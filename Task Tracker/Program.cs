using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Task_Tracker
{
    internal class TaskItem
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    internal class TaskManager
    {
        private List<TaskItem> tasks = new List<TaskItem>();
        private int nextId = 1;
        private const string FileName = "tasks.json";

        public TaskManager()
        {
            LoadTasks();
        }

        public void AddTask(string description)
        {
            var task = new TaskItem
            {
                Id = nextId++,
                Description = description,
                Status = TaskStatus.Pending,
                CreatedAt = DateTime.Now
            };
            tasks.Add(task);
            SaveTasks();
        }

        public List<TaskItem> ListTasks()
        {
            return tasks;
        }

        public bool CompleteTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null && task.Status == TaskStatus.Pending)
            {
                task.Status = TaskStatus.Completed;
                SaveTasks();
                return true;
            }
            return false;
        }

        public bool RemoveTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                tasks.Remove(task);
                SaveTasks();
                return true;
            }
            return false;
        }

        public bool EditTask(int id, string newDescription)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null && !string.IsNullOrWhiteSpace(newDescription))
            {
                task.Description = newDescription;
                SaveTasks();
                return true;
            }
            return false;
        }

        public bool SetTaskStatus(int id, TaskStatus status)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.Status = status;
                SaveTasks();
                return true;
            }
            return false;
        }

        private void SaveTasks()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tasks, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FileName, json);
        }

        private void LoadTasks()
        {
            if (!File.Exists(FileName))
                return;

            var json = File.ReadAllText(FileName);
            var loadedTasks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TaskItem>>(json);
            if (loadedTasks != null)
            {
                tasks = loadedTasks;
                if (tasks.Count > 0)
                    nextId = tasks.Max(t => t.Id) + 1;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var manager = new TaskManager();
            while (true)
            {
                Console.Clear();
                string input;
                do
                {
                    Console.WriteLine("\n=== Task Tracker ===");
                    Console.WriteLine("1. Adicionar tarefa");
                    Console.WriteLine("2. Listar tarefas");
                    Console.WriteLine("3. Concluir tarefa");
                    Console.WriteLine("4. Remover tarefa");
                    Console.WriteLine("5. Editar tarefa");
                    Console.WriteLine("6. Marcar tarefa como Fazendo");
                    Console.WriteLine("0. Sair");
                    Console.Write("Escolha uma opção: ");
                    input = Console.ReadLine();

                    if (input != "1" && input != "2" && input != "3" && input != "4" && input != "5" && input != "6" && input != "0")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        Console.ResetColor();
                    }
                } while (input != "1" && input != "2" && input != "3" && input != "4" && input != "5" && input != "6" && input != "0");

                Console.Clear();

                switch (input)
                {
                    case "1":
                        string desc;
                        do
                        {
                            Console.Write("Descrição da tarefa: ");
                            desc = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(desc))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("A descrição não pode ser vazia.");
                                Console.ResetColor();
                            }
                        } while (string.IsNullOrWhiteSpace(desc));
                        manager.AddTask(desc);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Tarefa adicionada!");
                        Console.ResetColor();
                        Pause();
                        break;
                    case "2":
                        while (true)
                        {
                            Console.WriteLine("\nFiltrar tarefas por:");
                            Console.WriteLine("1. Todas");
                            Console.WriteLine("2. Pendentes");
                            Console.WriteLine("3. Em andamento");
                            Console.WriteLine("4. Concluídas");
                            Console.WriteLine("0. Voltar");
                            Console.Write("Escolha uma opção: ");
                            var filterInput = Console.ReadLine();

                            var tasks = manager.ListTasks();
                            if (tasks.Count == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Nenhuma tarefa encontrada.");
                                Console.ResetColor();
                                Pause();
                                break;
                            }

                            List<TaskItem> filtered = null;
                            string titulo = "";

                            switch (filterInput)
                            {
                                case "1":
                                    filtered = tasks;
                                    titulo = "Todas as tarefas";
                                    break;
                                case "2":
                                    filtered = tasks.Where(t => t.Status == TaskStatus.Pending).ToList();
                                    titulo = "Tarefas Pendentes";
                                    break;
                                case "3":
                                    filtered = tasks.Where(t => t.Status == TaskStatus.Ongoing).ToList();
                                    titulo = "Tarefas Em andamento";
                                    break;
                                case "4":
                                    filtered = tasks.Where(t => t.Status == TaskStatus.Completed).ToList();
                                    titulo = "Tarefas Concluídas";
                                    break;
                                case "0":
                                    Console.Clear();
                                    goto EndList;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Opção inválida. Tente novamente.");
                                    Console.ResetColor();
                                    continue;
                            }

                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n{titulo}:");
                            Console.ResetColor();

                            if (filtered.Count == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Nenhuma tarefa encontrada.");
                                Console.ResetColor();
                            }
                            else
                            {
                                foreach (var t in filtered)
                                {
                                    string statusBox;
                                    ConsoleColor color;
                                    switch (t.Status)
                                    {
                                        case TaskStatus.Pending:
                                            statusBox = "[ ]";
                                            color = ConsoleColor.Yellow;
                                            break;
                                        case TaskStatus.Ongoing:
                                            statusBox = "[-]";
                                            color = ConsoleColor.Blue;
                                            break;
                                        case TaskStatus.Completed:
                                            statusBox = "[X]";
                                            color = ConsoleColor.Green;
                                            break;
                                        default:
                                            statusBox = "[?]";
                                            color = ConsoleColor.Gray;
                                            break;
                                    }
                                    Console.ForegroundColor = color;
                                    Console.Write($"{t.Id}: {statusBox} ");
                                    Console.ResetColor();
                                    Console.WriteLine($"{t.Description} (Criada em {t.CreatedAt:dd/MM/yyyy HH:mm})");
                                }

                                // Adicione aqui a opção de marcar como concluída
                                Console.Write("\nDeseja marcar alguma tarefa como concluída? (s/n): ");
                                var resp = Console.ReadLine();
                                if (resp?.Trim().ToLower() == "s")
                                {
                                    Console.Write("Digite o ID da tarefa para concluir: ");
                                    if (int.TryParse(Console.ReadLine(), out int idConcluir))
                                    {
                                        var tarefa = filtered.FirstOrDefault(t => t.Id == idConcluir);
                                        if (tarefa != null && tarefa.Status != TaskStatus.Completed)
                                        {
                                            if (manager.SetTaskStatus(idConcluir, TaskStatus.Completed))
                                            {
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Tarefa marcada como concluída!");
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("Não foi possível marcar como concluída.");
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("Tarefa não encontrada ou já está concluída.");
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("ID inválido.");
                                    }
                                    Console.ResetColor();
                                    Pause();
                                    Console.Clear();
                                }
                            }
                            Pause();
                            Console.Clear();
                        }
                    EndList:
                        break;
                    case "3":
                        Console.Write("ID da tarefa para concluir: ");
                        if (int.TryParse(Console.ReadLine(), out int completeId))
                        {
                            if (manager.CompleteTask(completeId))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Tarefa concluída!");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Tarefa não encontrada ou já concluída.");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ID inválido.");
                        }
                        Console.ResetColor();
                        Pause();
                        break;
                    case "4":
                        Console.Write("ID da tarefa para remover: ");
                        if (int.TryParse(Console.ReadLine(), out int removeId))
                        {
                            var task = manager.ListTasks().FirstOrDefault(t => t.Id == removeId);
                            if (task != null)
                            {
                                Console.Write($"Tem certeza que deseja remover a tarefa \"{task.Description}\"? (s/n): ");
                                var confirm = Console.ReadLine();
                                if (confirm?.Trim().ToLower() == "s")
                                {
                                    manager.RemoveTask(removeId);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Tarefa removida!");
                                }
                                else
                                {
                                    Console.WriteLine("Remoção cancelada.");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Tarefa não encontrada.");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ID inválido.");
                        }
                        Console.ResetColor();
                        Pause();
                        break;
                    case "5":
                        Console.Write("ID da tarefa para editar: ");
                        if (int.TryParse(Console.ReadLine(), out int editId))
                        {
                            var task = manager.ListTasks().FirstOrDefault(t => t.Id == editId);
                            if (task != null)
                            {
                                Console.WriteLine($"Descrição atual: {task.Description}");
                                string newDesc;
                                do
                                {
                                    Console.Write("Nova descrição: ");
                                    newDesc = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(newDesc))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("A descrição não pode ser vazia.");
                                        Console.ResetColor();
                                    }
                                } while (string.IsNullOrWhiteSpace(newDesc));
                                if (manager.EditTask(editId, newDesc))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Tarefa editada com sucesso!");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Falha ao editar a tarefa.");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Tarefa não encontrada.");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ID inválido.");
                        }
                        Console.ResetColor();
                        Pause();
                        break;
                    case "6":
                        Console.Write("ID da tarefa para marcar como Fazendo: ");
                        if (int.TryParse(Console.ReadLine(), out int doingId))
                        {
                            if (manager.SetTaskStatus(doingId, TaskStatus.Ongoing))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Tarefa marcada como Fazendo!");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Tarefa não encontrada.");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ID inválido.");
                        }
                        Console.ResetColor();
                        Pause();
                        break;
                    case "0":
                        Console.WriteLine("Saindo...");
                        return;
                }
            }
        }

        static void Pause()
        {
            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }

    internal enum TaskStatus
    {
        Pending,
        Ongoing,
        Completed
    }
}
