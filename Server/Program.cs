using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using NLog;
using System.Data.SqlClient;
using System.Net;
using System.Linq;


namespace Server
{
    class AsyncUdpServer
    {
        private const int Port = 8001;
        //private const string FilePath = "D:\\3_course\\IS_architecture\\IS_lab2.0\\Server\\data.csv";
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main()
        {
            UdpClient udpServer = new UdpClient(Port);
            Console.WriteLine("Сервер запущен и ждет подключения...");

            while (true)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                string request = Encoding.UTF8.GetString(result.Buffer);
                string response = "";

                if (request == "getall")
                {
                    response = ShowAll();
                    logger.Info("Вывод всех записей");
                }
                else if (request.StartsWith("get"))
                {
                    int id;

                    if (int.TryParse(request.Substring(4), out id))
                    {
                        response = ShowById(id);
                        logger.Info("Вывод записи по номеру");
                    }
                    else
                    {
                        response = "Некоректный номер строки, либо ее отсутствие";
                        logger.Info("Ошибка вывода записи по номеру");
                    }
                
                }
                else if (request.StartsWith("delete"))
                {
                    int id;
                    if (int.TryParse(request.Substring(7), out id))
                    {
                        response = DeleteById(id);
                        logger.Info("Удаление записи по номеру");
                    }
                    else
                    {
                        response = "Некоректный номер строки, либо ее отсутствие";
                        logger.Info("Ошибка удаления записи по номеру");
                    }


                }
                else if (request.StartsWith("add"))
                {
                    string data = request.Substring(4);
                    bool isAdded = AddRecord_db(data);

                    if (isAdded)
                    {
                        response = "Запись успешно добавлена.";
                        logger.Info("Запись успешно удалена");
                    }
                    else
                    {
                        response = "Ошибка при добавлении записи.";
                        logger.Info("Ошибка");
                    }
                }

                byte[] responseData = Encoding.UTF8.GetBytes(response);
                await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
            }          

        }
  

        //вывод всех записей для работы с БД MS SQL
        static string ShowAll()
        {
            string result = "";

            using (StudentsContext context = new StudentsContext())
            {
                var students = context.Students.ToList();

                foreach (var student in students)
                {
                    result += $"{student.Id},{student.Fname}, {student.Lname}, {student.Age}, {student.Status}\n";
                }
            }

            return result;
        }
        //вывод записи из базы по ID
        static string ShowById(int id)
        {
            string result = "";

            using (var dbContext = new StudentsContext())
            {
                var student = dbContext.Students.FirstOrDefault(s => s.Id == id);

                if (student != null)
                {
                    result += $"{student.Id},{student.Fname}, {student.Lname}, {student.Age}, {student.Status}\n";
                }
            }

            return result;
        }
        //удаление записи по ID
        static string DeleteById(int id)
        {
            using (var context = new StudentsContext())
            {
                var student = context.Students.FirstOrDefault(s => s.Id == id);

                if (student != null)
                {
                    context.Students.Remove(student);
                    context.SaveChanges();
                    return $"Запись с ID={id} успешно удалена";
                }
                else
                {
                    return $"Запись с ID={id} не найдена";
                }
            }

        }
        //Добавление записи в бд
        static bool AddRecord_db(string data)
        {
            string[] values = data.Split(',');

            if (values.Length != 5)
            {
                return false;
            }

            int id;
            string firstName;
            string lastName;
            int age;
            bool isActive;

            if (!int.TryParse(values[0].Trim(), out id) || id <= 0)
            {
                return false;
            }

            firstName = values[1].Trim();
            lastName = values[2].Trim();

            if (!int.TryParse(values[3].Trim(), out age) || age <= 0)
            {
                return false;
            }

            if (!bool.TryParse(values[4].Trim(), out isActive))
            {
                return false;
            }

            using (StudentsContext context = new StudentsContext())
            {
                if (context.Students.Any(s => s.Id == id))
                {
                    return false; // ID уже существует - запись не может быть добавлена
                }

                Student newStudent = new Student();
                newStudent.Id= id;
                newStudent.Fname = firstName;
                newStudent.Lname = lastName;
                newStudent.Age = age;
                newStudent.Status = isActive;

                context.Students.Add(newStudent);
                int rowsAffected = context.SaveChanges();

                return rowsAffected > 0;
            }
        }
    }

}

