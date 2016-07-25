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

    class Telefon
    {
        public Socket gniazdko { set; get; }
        public User user { set; get; }
        public bool started { get; set; }

        public Telefon(Socket socket)
        {
            user = new User();
            gniazdko = socket;
            started = true;
            Thread wątek = new Thread(odbierz_komunikat);
            wątek.Start();
        }

        string potnij(ref string bufor) //zwraca fragment stringa do napotkania spacji, obcina oryginalnego stringa o kawałek który jest zwracany
        {
            int i = 0;

            for (; i < bufor.Length; i++)
            {
                if (bufor[i] == ' ')
                {
                    break;
                }
            }

            string zwracanie = bufor.Substring(0, i);
            bufor = bufor.Substring(i + 1);
            return zwracanie;
        }

        void odbierz_komunikat()
        {
            while (started == true)
            {
                Komunikat komunikat = new Komunikat();

                byte[] bufor = new byte[256];
                try
                {
                    gniazdko.Receive(bufor);
                }
                catch
                {
                    Console.WriteLine("Klient " + user.nick + " niespodziewanie się rozłączył");

                    komunikat = Komunikaty.stwórz_komunikat("TERMINATE", "centrala");
                    komunikat.nadawca = user;
                    Program.KolejkaKomunikatów.Enqueue(komunikat);

                    started = false;
                    break;
                }

                komunikat = Komunikaty.odczytaj_komunikat(bufor);
                user.nick = komunikat.nadawca.nick;

                Console.WriteLine("Telefon " + komunikat.nadawca.nick + " wysłał komunikat \"" + komunikat.cseq.polecenie + "\" do " + komunikat.odbiorca.nick + ".");

                Program.KolejkaKomunikatów.Enqueue(komunikat);
            }
        }
    }
}
