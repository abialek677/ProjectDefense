using System.Net.Http.Json;
using ProjectDefense.Shared.DTOs;

namespace ProjectDefense.ConsoleApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5291/")
        };
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Project Defense - Klient Konsolowy ===\n");
            
            while (true)
            {
                Console.WriteLine("\nWybierz opcję:");
                Console.WriteLine("1. Wyświetl dostępne terminy");
                Console.WriteLine("2. Wyświetl sale");
                Console.WriteLine("3. Zarezerwuj termin");
                Console.WriteLine("4. Wyjście");
                Console.Write("\nTwój wybór: ");
                
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
                        return;
                    default:
                        Console.WriteLine("Nieprawidłowy wybór");
                        break;
                }
            }
        }
        
        static async Task DisplayAvailableSlots()
        {
            try
            {
                var response = await client.GetAsync("api/slots/available");
                
                if (response.IsSuccessStatusCode)
                {
                    var slots = await response.Content.ReadFromJsonAsync<List<SlotDto>>();
                    
                    Console.WriteLine("\n=== Dostępne terminy ===");
                    
                    if (slots == null || !slots.Any())
                    {
                        Console.WriteLine("Brak dostępnych terminów");
                        return;
                    }
                    
                    foreach (var slot in slots)
                    {
                        Console.WriteLine($"\nID: {slot.Id}");
                        Console.WriteLine($"Sala: {slot.NazwaSali}");
                        Console.WriteLine($"Data: {slot.CzasRozpoczecia:dd.MM.yyyy}");
                        Console.WriteLine($"Godziny: {slot.CzasRozpoczecia:HH:mm} - {slot.CzasZakonczenia:HH:mm}");
                        Console.WriteLine($"Prowadzący: {slot.ProwadzacyName}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
                else
                {
                    Console.WriteLine($"Błąd: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
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
                    
                    Console.WriteLine("\n=== Dostępne sale ===");
                    
                    if (rooms == null || !rooms.Any())
                    {
                        Console.WriteLine("Brak sal");
                        return;
                    }
                    
                    foreach (var room in rooms)
                    {
                        Console.WriteLine($"ID: {room.Id} | {room.Nazwa} (Nr: {room.NumerSali})");
                    }
                }
                else
                {
                    Console.WriteLine($"Błąd: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }
        
        static async Task BookSlot()
        {
            Console.Write("\nPodaj ID slotu do zarezerwowania: ");
            if (!int.TryParse(Console.ReadLine(), out int slotId))
            {
                Console.WriteLine("Nieprawidłowe ID");
                return;
            }
            
            Console.Write("Podaj ID studenta: ");
            var studentId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(studentId))
            {
                Console.WriteLine("ID studenta nie może być puste");
                return;
            }
            
            try
            {
                var bookingRequest = new BookingRequestDto
                {
                    StudentId = studentId
                };
                
                var response = await client.PostAsJsonAsync(
                    $"api/slots/{slotId}/book", 
                    bookingRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("\n✓ Rezerwacja zakończona sukcesem!");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"\n✗ Błąd rezerwacji: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }
    }
}
