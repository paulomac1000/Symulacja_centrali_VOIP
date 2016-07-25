using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Biblioteka;

namespace Centrala
{
    class Program
    {
        static IPAddress wybierz_IP() //pozwala użytkownikowi wybrać IP centrali ze wszystkich adresów IPv4 tego komputera
        {
            string NazwaHosta = Dns.GetHostName(); //pobieram nazwę komputera
            IPAddress[] ipLocal = Dns.GetHostAddresses(NazwaHosta); //pobieram wszystkie adresy IPv4 i 6 tego komputera

            int i = 0;
            Console.WriteLine("Znaleziono następujące adresy IPv4 dla " + NazwaHosta);
            Console.WriteLine("Wskaż, który adres ma zostać przypisany centrali.");

            //teraz filtruję adresy IPv4 od IPv6, v6 mnie nie interesują, v4 wrzucam na listę
            List<IPAddress> IPv4 = new List<IPAddress>();
            foreach (IPAddress bufor in ipLocal)
            {
                if (bufor.ToString().Contains(":") == false)
                {
                    IPv4.Add(bufor);
                    Console.Write("[" + i + "] ");
                    Console.WriteLine(bufor);
                    i++;
                }
            }
            //użytkownik wskazuje na którym ip chce mieć gniazdko
            int wybór = Int32.Parse(Console.ReadLine());
            return IPv4[wybór];
        }

        static void nasłuchuj_i_podłączaj() //funkcja nasłuchuje przychodzących połączeń do gniazda centrali, a gdy jakieś wykryje, przyjmuje je i tworzy z niego telefon
        {
            while(true)
            {
                Socket gniazdko = centrala.Accept();
                Console.WriteLine("Podłączono klienta!");

                Telefon telefon = new Telefon(gniazdko);

                ListaTelefonów.Add(telefon); //dodanie telefonu na listę wszystkich aktualnie połączonych telefonów

                Console.WriteLine("Aktualnie podłączono klientów: " + ListaTelefonów.Count);
            }
        }

        static public Socket centrala;
        volatile static public List<Telefon> ListaTelefonów;
        volatile static public Queue<Komunikat> KolejkaKomunikatów;
        static public IPPort IPPortserwera;

        static void Main(string[] args)
        {
            centrala = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tworzę gniazdko dla centrali
            //addressfamily - określa schemat adresowania, internetwork - ipv4
            ListaTelefonów = new List<Telefon>(); //lista na telefony
            KolejkaKomunikatów = new Queue<Komunikat>(); //kolejka na komunikaty

            IPPortserwera = new IPPort(wybierz_IP(),31230); //użytkownik wybiera adres centrali ze znalezionych adresów IPv4 komputera
            centrala.Bind(new IPEndPoint(IPPortserwera.address, IPPortserwera.port)); //przypisuję centrali adres IP wybrany przez użytkownika i sztywny port 31320

            Console.Clear();
            Console.WriteLine("Uruchomiono centralę pod adresem " + IPPortserwera.address + ".");
            centrala.Listen(999); //maksymalnie 999 klintów może czekać na połączenie

            /*************************/
            //tutaj tworzę wątek który nasłuchuje czy ktoś się chce połączyć z centralą
            //jeżeli ktoś zechce się połączyć, tworzę obiekt klasy telefon z własnym gniazdkiem
            //telefon dostaje swój wątek do nasłuhiwania czy jest wysyłany jakiś komunikat do jego gniazdka
            Thread podłączanie = new Thread(nasłuchuj_i_podłączaj);
            podłączanie.Start();
            /*************************/

            /*************************/
            //tutaj tworzę wątek który przetwarza otrzymane komunikaty
            Thread komunikaty = new Thread(Komunikaty.obsłuż_komunikaty);
            komunikaty.Start();
            /*************************/

            while(true)
            {
                Console.WriteLine("Wpisz polecenie do konsoli centrali:");
                string wybór = Console.ReadLine();

                switch(wybór)
                {
                    case "IP":
                        Console.WriteLine("IP centrali to "+ IPPortserwera.address);
                        break;

                    case "podłączone":
                        Console.WriteLine("Aktualnie podłączono " + ListaTelefonów.Count + " telefonów.");
                        foreach (Telefon telefon in ListaTelefonów)
                        {
                            Console.WriteLine("Telefon o nicku "+ telefon.user.nick);
                        }
                        break;

                    case "?":
                        Console.WriteLine("[IP] pokaż IP centrali.");
                        Console.WriteLine("[connecton] pokaż ilość podłączonych telefonów oraz ich adresy IP.");
                        Console.WriteLine("[rozłącz] rozłącz telefon.");
                        break;

                    case "rozłącz":
                        Console.WriteLine("Podaj nick telefonu, który chcesz rozłączyć: ");
                        wybór = Console.ReadLine();
                        Komunikat komunikat = Komunikaty.stwórz_komunikat("TERMINATE","centrala");
                        komunikat.nadawca.nick = wybór;
                        Komunikaty.wyślij_komunikat(komunikat);
                        break;

                    default:
                        Console.WriteLine("Nie rozpoznano polecenia " + wybór + ". Wpisz '?' aby uzyskać pomoc.");
                        break;
                }
            }
        }
    }
}
