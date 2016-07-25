using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Telefon
{
    class Program
    {
        static IPAddress wybierz_IP() //pozwala użytkownikowi wybrać IP centrali ze wszystkich adresów IPv4 tego komputera
        {
            string NazwaHosta = Dns.GetHostName(); //pobieram nazwę komputera
            IPAddress[] ipLocal = Dns.GetHostAddresses(NazwaHosta); //pobieram wszystkie adresy IPv4 i 6 tego komputera

            int i = 0;
            Console.WriteLine("Znaleziono następujące adresy IPv4 dla " + NazwaHosta);
            Console.WriteLine("Wskaż, który adres ma zostać przypisany do telefonu.");

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
            int wybór = Convert.ToInt32(Console.ReadLine());

            return IPv4[wybór];
        }
        static public int czeka_na_odpowiedź()
        {
            while (odpowiedź == 0)
            {
                Console.WriteLine("Czekam na odpowiedź...");
                Thread.Sleep(1000);
            }
            int bufor = odpowiedź;
            odpowiedź=0;
            return bufor;
        }
        public static int wybór_usługi()
        {
            Console.WriteLine("Jaki typ transmisji chcesz użyć?\n (d - dane, a - audio, w - wideo)");

            ConsoleKeyInfo wybór = Console.ReadKey();

            switch (wybór.KeyChar)
            {
                case 'd':
                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DANE", Połączenie.rozmówca.nick));
                    Połączenie.rodzaj_usług = 'd';
                    return 11;
                case 'a':
                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("AUDIO", Połączenie.rozmówca.nick));
                    Połączenie.rodzaj_usług = 'a';
                    return 12;
                case 'w':
                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("WIDEO", Połączenie.rozmówca.nick));
                    Połączenie.rodzaj_usług = 'w';
                    return 13;
                default:
                    Console.WriteLine("Użytkownik wybrał zły znak");
                    return wybór_usługi();
            }
        }

        public static Socket telefon;
        volatile static public Queue<Komunikat> KolejkaKomunikatów;
        public static IPPort IPPortserwera;
        public static IPPort IPPorttelefonu;
        volatile public static int odpowiedź = 0;


        static void Main(string[] args)
        {


            telefon = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.Write("Podaj nick telefonu: ");

            Połączenie.user.nick = Console.ReadLine();

            Połączenie.user.mail = Połączenie.user.nick + "@domena.pl";
            IPPorttelefonu = new IPPort(wybierz_IP(),31231);
            telefon.Bind(new IPEndPoint(IPPorttelefonu.address, IPPorttelefonu.port));
            Console.Write("Podaj adres IP serwera: ");

            string IP = Console.ReadLine();

            IPPortserwera = new IPPort(IPAddress.Parse(IP),31230);

            try
            {
                telefon.Connect(IPPortserwera.address, IPPortserwera.port);
            }
            catch
            {
                Console.WriteLine("Nie można nawiązać połączenia.");
                Main(args);
            }
            Console.WriteLine("Połączono!");

            Połączenie.stan = "dostępny";

            KolejkaKomunikatów = new Queue<Komunikat>(); //kolejka na komunikaty
            /*************************/
            //tutaj tworzę wątek który odbiera wysyłane komunikaty z centrali
            Thread komunikaty = new Thread(Komunikaty.odbierz_komunikat);
            komunikaty.Start();
            /*************************/

            /*************************/
            //tutaj tworzę wątek który osługuje komunikaty będące w kolejce komunikatów
            Thread obsługa = new Thread(Komunikaty.obsłuż_komunikaty);
            obsługa.Start();
            /*************************/

            //wysyłam komunikat REGISTER
            Komunikat komunikat = Komunikaty.stwórz_komunikat("REGISTER", "centrala");
            Komunikaty.wyślij_komunikat(komunikat);

            czeka_na_odpowiedź();

            while (true)
            {
                Console.WriteLine("Wpisz polecenie do konsoli telefonu:");
    
                string wybór = Console.ReadLine();
    

                switch (wybór)
                {
                    case "telefon":
                        Console.WriteLine("IP telefonu to " + IPPorttelefonu.address);
                        break;

                    case "centrala":
                        Console.WriteLine("IP centrali to " + IPPortserwera.address);
                        break;

                    case "połącz":                     
                        if(Połączenie.stan == "dostępny")
                        {
                            Console.WriteLine("Podaj nick telefonu z którym chcesz się połączyć: ");
                            Połączenie.rozmówca = new User();

                            Połączenie.rozmówca.nick = Console.ReadLine();

                            Połączenie.stan = "łączenie";

                            Połączenie.rozmówca.mail = Połączenie.rozmówca.nick + "@domena.pl";
                            Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("CONNECT", Połączenie.rozmówca.nick));

                            Połączenie.blokada_wysłania_odpowiedzi = true;
                            switch (czeka_na_odpowiedź())
                            {
                                case 1:
                                    int usługa = wybór_usługi();
                                    if(czeka_na_odpowiedź()==1)
                                    {
                                        switch (usługa)
                                        {
                                            case 11:
                                                Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("PORT;31232", Połączenie.rozmówca.nick));
                                                Połączenie.port = 31232;
                                                break;
                                            case 12:
                                                Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("PORT;31233", Połączenie.rozmówca.nick));
                                                Połączenie.port = 31233;
                                                break;
                                            case 13:
                                                Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("PORT;31234", Połączenie.rozmówca.nick));
                                                Połączenie.port = 31234;
                                                break;
                                            default:
                                                break;
                                        }
                                        if (czeka_na_odpowiedź() == 1)
                                        {
                                            switch (usługa)
                                            {
                                                case 11:
                                                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("FORMATDANYCH;Dane1:1,2,3;Dane2:1,2,3;Dane3:1,2,3;Dane4:1,2,3;", Połączenie.rozmówca.nick));
                                                    break;
                                                case 12:
                                                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("FORMATAUDIO;Audio1:MP3,44100,2;Audio2:OGG,45000,1;Audio3:AC3,32000,2;Audio4:WAV,44100,2;", Połączenie.rozmówca.nick));
                                                    break;
                                                case 13:
                                                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("FORMATWIDEO;Wideo1:RMVB,30,1920x1080;Wideo2:AVI,23,640X480;Wideo3:MPEG4,20,320x480;Wideo4:MOV,23,720x480;", Połączenie.rozmówca.nick));
                                                    break;
                                                default:
                                                    break;
                                            }
                                            Połączenie.stan = "zajęty";
                                            Thread.Sleep(100); //czekam 100ms aby jakiś inny wątek nie zapisał konsoli po poniższym napisie; poniższy napis ma być ostatni
                                            Console.WriteLine("Nawiązano połączenie!");
                                            Połączenie.blokada_wysłania_odpowiedzi = false;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Nieprawidłowa odpowiedź na numer portu");
                                            Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nieprawidłowa odpowiedź na wybór usługi");
                                        Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                                    }
                                    break;
                                case 2:
                                    Console.WriteLine("Telefon, z którym próbujesz się połączyć jest zajęty.");
                                    break;
                                case 3:
                                    Console.WriteLine("Centrala nie znalazła użytkownika o zadanym nicku.");
                                    break;
                                case 4:
                                    Console.WriteLine("Telefon docelowy odrzucił Twoje połączenie.");
                                    break;
                                default:
                                    Console.WriteLine("Nieprawidłowa odpowiedź");
                                    Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Już jesteś połączony z jakimś telefonem");
                        }
                        break;
                    case "rozłącz":
                        if (Połączenie.stan != "dostępny")
                        {
                            Połączenie.stan = "dostępny";
                            Połączenie.rodzaj_usług = '-';
                            Połączenie.port = 0;
                            Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                        }
                        else
                        {
                            Console.WriteLine("Nie jesteś połączony z żadnym telefonem");
                        }
                        break;
                    case "zakończ":
                        if (Połączenie.stan == "zajęty")
                        {
                            Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                        }
                        Komunikaty.wyślij_komunikat(Komunikaty.stwórz_komunikat("TERMINATE", "centrala"));
                        break;
                    case "port":
                        Połączenie.odczytaj_port();
                        break;
                    case "usługa":
                        Połączenie.odczytaj_informacje_o_usłudze();
                        break;
                    case "t":
                        if (Połączenie.stan == "łączenie")
                        {
                            Połączenie.stan = "zaakceptowano";
                        }
                        else
                        {
                            Console.WriteLine("Nie rozpoznano polecenia " + wybór + ". Wpisz '?' aby uzyskać pomoc.");
                        }
                        break;
                    case "n":
                        if (Połączenie.stan == "łączenie")
                        {
                            Połączenie.stan = "odrzucono";
                        }
                        else
                        {
                            Console.WriteLine("Nie rozpoznano polecenia " + wybór + ". Wpisz '?' aby uzyskać pomoc.");
                        }
                        break;
                    case "?":
                        Console.WriteLine("[telefon] pokaż IP telefonu.");
                        Console.WriteLine("[centrala] pokaż IP centrali.");
                        Console.WriteLine("[połącz] połącz się z telefonem o podanym nicku.");
                        Console.WriteLine("[rozłącz] rozłącz trwające połączenie z telefonem.");
                        Console.WriteLine("[zakończ] zakończ program.");
                        break;

                    default:
                        Console.WriteLine("Nie rozpoznano polecenia " + wybór + ". Wpisz '?' aby uzyskać pomoc.");
                        break;
                }
            }
        }
    }
}
