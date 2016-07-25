using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telefon
{
    class Połączenie
    {
        public static string stan = "dostępny";
        public static User user = new User();
        public static User rozmówca = new User();
        public static char rodzaj_usług = '-';
        public static int port = 0;
        public static bool blokada_wysłania_odpowiedzi = false;


        public struct Dane
        {
            public string argument1;
            public string argument2;
            public string argument3;
        }

        public struct Audio
        {
            public string kodek;
            public string próbkowanie;
            public string liczba_kanałów;
        }

        public struct Wideo
        {
            public string kodek;
            public string liczba_klatek;
            public string rozdzielczość;
        }

        public static Dane dane = new Dane();
        public static Audio audio = new Audio();
        public static Wideo wideo = new Wideo();

        public static void zapisz_dane(string bufor)
        {
            string[] tab = new string[3];
            int i = 0;
            while (bufor[i] != ':')
            {
                i++;
            }
            i++;
            for (int j = 0; i < bufor.Length; i++)
            {
                if (bufor[i] != ',')
                {
                    tab[j] += bufor[i];
                }
                else
                {
                    j++;
                }
            }

            dane.argument1 = tab[0];
            dane.argument2 = tab[1];
            dane.argument3 = tab[2];
        }

        public static void zapisz_audio(string bufor)
        {
            string[] tab = new string[3];
            int i = 0;
            while (bufor[i] != ':')
            {
                i++;
            }
            i++;
            for (int j = 0; i < bufor.Length; i++)
            {
                if (bufor[i] != ',')
                {
                    tab[j] += bufor[i];
                }
                else
                {
                    j++;
                }
            }

            audio.kodek = tab[0];
            audio.próbkowanie = tab[1];
            audio.liczba_kanałów = tab[2];
        }

        public static void zapisz_wideo(string bufor)
        {
            string[] tab = new string[3];
            int i = 0;
            while (bufor[i] != ':')
            {
                i++;
            }
            i++;
            for (int j = 0; i < bufor.Length; i++)
            {
                if (bufor[i] != ',')
                {
                    tab[j] += bufor[i];
                }
                else
                {
                    j++;
                }
            }

            wideo.kodek = tab[0];
            wideo.liczba_klatek = tab[1];
            wideo.rozdzielczość = tab[2];
        }

        public static void odczytaj_informacje_o_usłudze()
        {
            if (rodzaj_usług == 'd')
            {
                if (dane.argument1 != null)
                {
                    Console.WriteLine("Informacje o transmisji danych: \n argument1: " + dane.argument1 + "\n argument2: " + dane.argument2 + "\n argument3: " +  dane.argument3);
                }
                else
                {
                    Console.WriteLine("Nie zdefiniowano jeszcze informacji o usłudze danych");
                }
            }
            else if (rodzaj_usług == 'a')
            {
                if (audio.kodek != null)
                {
                    Console.WriteLine("Informacje o transmisji audio: \n kodek: " + audio.kodek + "\n częstotliwość próbkowania: " + audio.próbkowanie + "\n liczba kanałów: " + audio.liczba_kanałów);
                }
                else
                {
                    Console.WriteLine("Nie zdefiniowano jeszcze informacji o usłudze audio");
                }
            }
            else if (rodzaj_usług == 'w')
            {
                if (wideo.kodek != null)
                {
                    Console.WriteLine("Informacje o transmisji wideo: \n kodek: " + wideo.kodek + "\n liczba klatek/s: " + wideo.liczba_klatek + "\n rozdzielczość: " + wideo.rozdzielczość);
                }
                else
                {
                    Console.WriteLine("Nie zdefiniowano jeszcze informacji o usłudze wideo.");
                }
            }
            else
            {
                Console.WriteLine("Nie nawiązano jeszcze połączenia!");
            }

        }

        public static void zapisz_port(int bufor)
        {
            port = bufor;
        }

        public static void odczytaj_port()
        {
            if (rodzaj_usług == 'd' || rodzaj_usług == 'a' || rodzaj_usług == 'w')
            {
                if (port != 0)
                {
                    Console.WriteLine("Port dla wybranej usługi to {0}", port);
                }
                else
                {
                    Console.WriteLine("Nie wybrano jeszcze portu dla danej usługi.");
                }
            }
            else
            {
                Console.WriteLine("Nie nawiązano jeszcze połączenia!");
            }
        }
    }
}
