using System;
using System.Threading.Tasks;
using SistemadeTiquetess.src.modules.BoardingPasses.Application.Interfaces;

namespace SistemadeTiquetess.src.modules.BoardingPasses.UI;

public class CheckInMenu
{
    private readonly IBoardingPassesService _service;

    public CheckInMenu(IBoardingPassesService service)
    {
        _service = service;
    }

    public async Task ShowMenuAsync()
    {
        bool isRunning = true;
        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("=========================================");
            Console.WriteLine("        Check-in y Pase de Abordar       ");
            Console.WriteLine("=========================================");
            Console.WriteLine("1. Realizar Check-in (Generar Pase)");
            Console.WriteLine("2. Consultar Pase de Abordar");
            Console.WriteLine("3. Consultar Pasajeros Listos para Abordar");
            Console.WriteLine("4. Volver");
            Console.WriteLine("=========================================");
            Console.Write("Seleccione: ");

            switch (Console.ReadLine())
            {
                case "1": await PerformCheckInAsync(); break;
                case "2": await ViewBoardingPassAsync(); break;
                case "3": await QueryReadyToBoardAsync(); break;
                case "4": isRunning = false; break;
                default: 
                    Console.WriteLine("Opción no válida.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private async Task PerformCheckInAsync()
    {
        Console.Clear();
        Console.WriteLine("--- Proceso de Check-in ---");
        Console.Write("Ingrese el número de tiquete o ID de reserva: ");
        string identifier = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(identifier))
        {
            Console.WriteLine("Identificador inválido.");
            Console.ReadKey();
            return;
        }

        try
        {
            // 1. Buscar y mostrar los datos del pasajero y del vuelo
            var details = await _service.GetCheckInDetailsAsync(identifier);
            
            Console.WriteLine("\n--- DATOS PARA EL CHECK-IN ---");
            Console.WriteLine($"Pasajero:    {details.PassengerName}");
            Console.WriteLine($"Vuelo:       {details.FlightCode}");
            Console.WriteLine($"Origen:      {details.Origin}");
            Console.WriteLine($"Destino:     {details.Destination}");
            Console.WriteLine($"Salida:      {details.DepartureTime}");
            Console.WriteLine($"Tiquete:     {details.TicketNumber}");
            Console.WriteLine($"Estado:      {details.TicketStatus}");
            Console.WriteLine($"Asiento:     {details.SeatNumber}");
            
            Console.WriteLine("\n¿Desea proceder con el Check-in? (S/N): ");
            var confirmation = Console.ReadLine()?.ToUpper();

            if (confirmation == "S")
            {
                // 2. Realizar validaciones y check-in
                var bp = await _service.ProcessCheckInAsync(identifier);
                
                Console.WriteLine("\n--- CHECK-IN EXITOSO ---");
                Console.WriteLine("--- PASE DE ABORDAR GENERADO ---");
                Console.WriteLine($"Código:           {bp.BoardingCode}");
                Console.WriteLine($"Puerta:           {bp.Gate}");
                Console.WriteLine($"Asiento:          {bp.Seat}");
                Console.WriteLine($"Hora de abordaje: {bp.BoardingTime}");
                Console.WriteLine($"Estado:           {bp.Status}");
                Console.WriteLine("\nEl estado del tiquete se ha actualizado a 'Check-in realizado'.");
            }
            else
            {
                Console.WriteLine("\nProceso cancelado por el usuario.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError en el proceso: {ex.Message}");
        }

        Console.WriteLine("\nPresione tecla para continuar...");
        Console.ReadKey();
    }

    private async Task ViewBoardingPassAsync()
    {
        Console.Clear();
        Console.WriteLine("--- Consultar Pase de Abordar ---");
        Console.Write("Ingrese el número de tiquete o ID de reserva: ");
        string identifier = Console.ReadLine() ?? "";

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            try
            {
                var bp = await _service.GetBoardingPassAsync(identifier);
                if (bp != null)
                {
                    Console.WriteLine("\n--- PASE DE ABORDAR ---");
                    Console.WriteLine($"Código: {bp.BoardingCode}");
                    Console.WriteLine($"Puerta: {bp.Gate}");
                    Console.WriteLine($"Asiento: {bp.Seat}");
                    Console.WriteLine($"Hora de abordaje: {bp.BoardingTime}");
                    Console.WriteLine($"Estado: {bp.Status}");
                }
                else
                {
                    Console.WriteLine("\nNo se encontró un pase de abordar para este identificador.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Identificador inválido.");
        }
        Console.WriteLine("\nPresione tecla para continuar...");
        Console.ReadKey();
    }

    private async Task QueryReadyToBoardAsync()
    {
        Console.Clear();
        Console.WriteLine("--- Consultar Pasajeros Listos para Abordar ---");
        Console.Write("Ingrese el ID del Vuelo (GUID): ");
        if (Guid.TryParse(Console.ReadLine(), out Guid flightId))
        {
            try
            {
                var passengers = await _service.GetReadyToBoardAsync(flightId);
                Console.WriteLine("\nLista de pases de abordar listos:");
                int count = 0;
                foreach (var p in passengers)
                {
                    Console.WriteLine($"- Código: {p.BoardingCode} | Asiento: {p.Seat} | Puerta: {p.Gate}");
                    count++;
                }
                
                if(count == 0) Console.WriteLine("No hay pasajeros listos para este vuelo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("ID inválido.");
        }
        Console.WriteLine("\nPresione tecla para continuar...");
        Console.ReadKey();
    }
}
