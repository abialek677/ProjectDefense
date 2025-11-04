using System.Net.Http.Json;
using ProjectDefense.Shared.DTOs;

namespace ProjectDefense.ConsoleApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5291/")
        };

        private static string? studentId; // zapamiętane ID użytkownika

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Project Defense - Console Client ===\n");

            await Login();

            while (true)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Show available slots");
                Console.WriteLine("2. Show rooms");
                Console.WriteLine("3. Book a slot");
                Console.WriteLine("4. Change student ID");
                Console.WriteLine("5. Exit");
                Console.Write("\nYour choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await DisplayAvailableSlots();
                        break;
                    case "2":
                        await DisplayRooms();
                        break;
                    case "3":
                        await BookSlot();
                        break;
                    case "4":
                        await Login();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        static async Task Login()
        {
            Console.Write("Enter your student ID: ");
            studentId = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(studentId))
            {
                Console.Write("Student ID cannot be empty, try again: ");
                studentId = Console.ReadLine();
            }

            Console.WriteLine($"Welcome, student ID: {studentId}");
        }

        static async Task DisplayAvailableSlots()
        {
            try
            {
                var response = await client.GetAsync("api/slots/available");

                if (response.IsSuccessStatusCode)
                {
                    var slots = await response.Content.ReadFromJsonAsync<List<SlotDto>>();

                    Console.WriteLine("\n=== Available Slots ===");

                    if (slots == null || !slots.Any())
                    {
                        Console.WriteLine("No available slots");
                        return;
                    }

                    foreach (var slot in slots)
                    {
                        Console.WriteLine($"\nID: {slot.Id}");
                        Console.WriteLine($"Room: {slot.RoomName}");
                        Console.WriteLine($"Date: {slot.StartTime:dd.MM.yyyy}");
                        Console.WriteLine($"Time: {slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}");
                        Console.WriteLine($"Instructor: {slot.InstructorName}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task DisplayRooms()
        {
            try
            {
                var response = await client.GetAsync("api/rooms");

                if (response.IsSuccessStatusCode)
                {
                    var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>();

                    Console.WriteLine("\n=== Available Rooms ===");

                    if (rooms == null || !rooms.Any())
                    {
                        Console.WriteLine("No rooms available");
                        return;
                    }

                    foreach (var room in rooms)
                    {
                        Console.WriteLine($"ID: {room.Id} | {room.Name} (Room no: {room.RoomNumber})");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task BookSlot()
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                Console.WriteLine("Student ID not set. Please log in first.");
                await Login();
            }

            Console.Write("\nEnter the slot ID to book: ");
            if (!int.TryParse(Console.ReadLine(), out int slotId))
            {
                Console.WriteLine("Invalid ID");
                return;
            }

            try
            {
                var bookingRequest = new BookingRequestDto
                {
                    StudentId = studentId!
                };

                var response = await client.PostAsJsonAsync(
                    $"api/slots/{slotId}/book",
                    bookingRequest
                );

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("\nBooking completed successfully!");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"\nBooking error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
