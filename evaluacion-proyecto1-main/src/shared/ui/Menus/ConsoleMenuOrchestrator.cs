using System;
using SistemadeTiquetess.src.shared.helpers;
using SistemadeTiquetess.src.modules.BoardingPasses.Infrastructure.Repositories;
using SistemadeTiquetess.src.modules.BoardingPasses.Application.Services;
using SistemadeTiquetess.src.modules.BoardingPasses.UI;

namespace SistemadeTiquetess.src.shared.ui.Menus;

public class ConsoleMenuOrchestrator
{
    public void Start()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("       SISTEMA DE TIQUETES             ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Gestión de Aerolíneas");
            Console.WriteLine("2. Gestión de Aeronaves");
            Console.WriteLine("3. Gestión de Vuelos");
            Console.WriteLine("4. Gestión de Clientes");
            Console.WriteLine("5. Gestión de Reservas y Tiquetes");
            Console.WriteLine("6. Configuración Geográfica (Ciudades, Países)");
            Console.WriteLine("7. Check-in y Pase de Abordar (Examen 3)");
            Console.WriteLine("0. Salir");
            Console.WriteLine("========================================");
            Console.Write("Seleccione una opción: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Aerolíneas");
                    Pause();
                    break;
                case "2":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Aeronaves");
                    Pause();
                    break;
                case "3":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Vuelos");
                    Pause();
                    break;
                case "4":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Clientes");
                    Pause();
                    break;
                case "5":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Reservas y Tiquetes");
                    Pause();
                    break;
                case "6":
                    Console.WriteLine("\n[Aún no implementado] Módulo de Configuración");
                    Pause();
                    break;
                case "7":
                    using (var context = DbContextFactory.Create())
                    {
                        var bpRepository = new BoardingPassesRepository(context);
                        var bpService = new BoardingPassesService(bpRepository, context);
                        var checkInMenu = new CheckInMenu(bpService);
                        checkInMenu.ShowMenuAsync().Wait();
                    }
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("\nSaliendo del orquestador de menús...");
                    break;
                default:
                    Console.WriteLine("\nOpción no válida. Intente nuevamente.");
                    Pause();
                    break;
            }
        }
    }

    private void Pause()
    {
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }
}
