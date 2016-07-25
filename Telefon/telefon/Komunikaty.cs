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
    class Komunikat
    {
        public IPPort przez { get; set; }
        public User nadawca { set; get; }
        public User odbiorca { set; get; }
        public CallID callid { get; set; }
        public CSeq cseq { get; set; }
        public Contact contact { get; set; }
        public string content_type { get; set; }
        public int content_length { get; set; }

        public string getString()
        {
            string bufor = przez.address.ToString() + ' ';
            bufor += przez.port.ToString() + ' ';
            bufor += nadawca.nick + ' ';
            bufor += nadawca.mail + ' ';
            bufor += odbiorca.nick + ' ';
            bufor += odbiorca.mail + ' ';
            bufor += callid.callid + ' ';
            bufor += cseq.numer.ToString() + ' ';
            bufor += cseq.polecenie + ' ';
            bufor += contact.nick + ' ';
            bufor += contact.mail + ' ';
            bufor += content_type + ' ';
            bufor += content_length.ToString() + ' ';

            return bufor;
        }
        public Komunikat()
        {
            przez = new IPPort("0.0.0.0:0");
            nadawca = new User();
            odbiorca = new User();
            callid = new CallID();
            cseq = new CSeq();
            contact = new Contact();
        }
    }

    class Komunikaty
    {
        static string potnij(ref string bufor) //zwraca fragment stringa do napotkania spacji, obcina oryginalnego stringa o kawałek który jest zwracany
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
            if (i < bufor.Length)
                bufor = bufor.Substring(i + 1);

            return zwracanie;
        }

        public static string potnij_kodeki(string bufor)
        {
            string[] tab = new string[5];
            int j = 0;
            //przewijam stringa aż do ';'
            for (int i = 0; i < bufor.Length; i++)
            {
                if (bufor[i] != ';')
                {
                    tab[j] += bufor[i];
                }
                else
                {
                    j++;
                }
            }
            //jeżeli jest tylko jeden kodek to znaczy, że drugi telefon odpowiedział na wysłane kodeki odsyłając jeden, który wybrał
            //wtedy zapisuję info o tym kodeku a funkcja nie musi nic zwracać
            if (j == 1)
            {
                switch (Połączenie.rodzaj_usług)
                {
                    case 'd':
                        Połączenie.zapisz_dane(tab[1]);
                        break;
                    case 'a':
                        Połączenie.zapisz_audio(tab[1]);
                        break;
                    case 'w':
                        Połączenie.zapisz_wideo(tab[1]);
                        break;
                    default:
                        Console.WriteLine("Nie wybrano usługi.");
                        break;
                }
                return tab[1];
            }
            //losuję który kodek wybrać
            Random rnd = new Random();
            int wylosowany = rnd.Next(1, j - 1);
            Console.WriteLine("Wybrano kodek: {0}", tab[wylosowany]);
            return tab[wylosowany];
        }

        public static Komunikat odczytaj_komunikat(byte[] komunikat)
        {
            Komunikat bufor = new Komunikat();

            string zawartość = Encoding.ASCII.GetString(komunikat);

            bufor.przez = new IPPort(IPAddress.Parse(potnij(ref zawartość)), Convert.ToInt32(potnij(ref zawartość)));
            bufor.nadawca.nick = potnij(ref zawartość);
            bufor.nadawca.mail = potnij(ref zawartość);
            bufor.odbiorca.nick = potnij(ref zawartość);
            bufor.odbiorca.mail = potnij(ref zawartość);
            bufor.callid = new CallID(potnij(ref zawartość));
            bufor.cseq.numer = Convert.ToInt32(potnij(ref zawartość));
            bufor.cseq.polecenie = potnij(ref zawartość);
            bufor.contact.nick = potnij(ref zawartość);
            bufor.contact.mail = potnij(ref zawartość);
            bufor.content_type = potnij(ref zawartość);
            bufor.content_length = Convert.ToInt32(potnij(ref zawartość));
            return bufor;
        }
        public static Komunikat stwórz_komunikat(string komenda, string nick)
        {
            Komunikat komunikat = new Komunikat();

            komunikat.przez = Program.IPPortserwera;
            komunikat.nadawca.nick = Połączenie.user.nick;
            komunikat.nadawca.mail = Połączenie.user.mail;
            komunikat.odbiorca.nick = nick;
            komunikat.odbiorca.mail = nick + "@domena.pl";

            komunikat.callid = new CallID();
            switch (komenda)//do rozszerzenia o pozostałe metody SIP
            {
                case "ACK":
                    komunikat.cseq.numer = 1;
                    break;
                case "BUSY":
                    komunikat.cseq.numer = 2;
                    break;
                case "NOTFOUND":
                    komunikat.cseq.numer = 3;
                    break;
                case "REJECT":
                    komunikat.cseq.numer = 4;
                    break;
                case "FORWARD":
                    komunikat.cseq.numer = 5;
                    break;
                case "DANE":
                    komunikat.cseq.numer = 11;
                    break;
                case "AUDIO":
                    komunikat.cseq.numer = 12;
                    break;
                case "WIDEO":
                    komunikat.cseq.numer = 13;
                    break;
                case "TERMINATE":
                    komunikat.cseq.numer = 21;
                    break;
                case "DISCONNECT":
                    komunikat.cseq.numer = 22;
                    break;
                case "CONNECT":
                    komunikat.cseq.numer = 23;
                    break;
                case "REGISTER":
                    komunikat.cseq.numer = 6;
                    break;

                default:
                    {
                        bool znaleziono = false;
                        if (komenda.Contains(';'))
                        {
                            znaleziono = true;

                            string format = komenda.Substring(0, komenda.IndexOf(';'));
                            switch (format)
                            {
                                case "FORMATDANYCH":
                                    komunikat.cseq.numer = 31;
                                    break;
                                case "FORMATAUDIO":
                                    komunikat.cseq.numer = 32;
                                    break;
                                case "FORMATWIDEO":
                                    komunikat.cseq.numer = 33;
                                    break;
                                case "PORT":
                                    komunikat.cseq.numer = 41;
                                    break;
                                default:
                                    Console.WriteLine("Tworzysz nierozpoznany komunikat ze średnikiem, nastąpi nadanie wartości deafaultowej '404'");
                                    komunikat.cseq.numer = 404;
                                    break;
                            }
                        }
                        if (znaleziono == false)
                        {
                            Console.WriteLine("Tworzysz nierozpoznany komunikat, nastąpi nadanie wartości deafaultowej '404'");
                            komunikat.cseq.numer = 404;
                        }
                        break;
                    }

            }
            komunikat.cseq.polecenie = komenda;
            komunikat.contact.nick = komunikat.nadawca.nick;
            komunikat.contact.mail = komunikat.nadawca.mail;
            komunikat.content_type = "application/sdp";
            komunikat.content_length = komunikat.getString().Length;

            return komunikat;
        }
        public static bool wyślij_komunikat(Komunikat komunikat)
        {
            byte[] bufor = new byte[komunikat.getString().Length];
            bufor = Encoding.ASCII.GetBytes(komunikat.getString());

            try
            {
                Program.telefon.Send(bufor);
            }
            catch
            {
                return false;
            }

            Console.WriteLine("Wysłano komunikat " + komunikat.cseq.polecenie + " do telefonu " + komunikat.odbiorca.nick);

            return true;
        }

        public static void odbierz_komunikat()
        {
            Komunikat komunikat = new Komunikat();

            byte[] bufor = new byte[256];

            while (true)
            {
                try
                {
                    Program.telefon.Receive(bufor);
                }
                catch
                {
                    Console.WriteLine("Centrala została nieoczekiwanie zatrzymana!");
                    return;
                }

                komunikat = odczytaj_komunikat(bufor);

                Console.WriteLine("Urządzenie " + komunikat.nadawca.nick + " wysłało komunikat \"" + komunikat.cseq.polecenie + "\" do " + komunikat.odbiorca.nick + ".");

                Program.KolejkaKomunikatów.Enqueue(komunikat);
            }
        }

        static public void obsłuż_komunikaty()
        {
            while (true)
            {
                if (Program.KolejkaKomunikatów.Count > 0)
                {
                    //zdejmuję z kolejki komunikatów komunikat, jeżeli tam jest
                    Komunikat komunikat = Program.KolejkaKomunikatów.Dequeue();

                    switch (komunikat.cseq.numer)
                    {
                        case 1:
                            Program.odpowiedź = 1;
                            break;
                        case 2:
                            Program.odpowiedź = 2;
                            break;
                        case 3:
                            Program.odpowiedź = 3;
                            break;
                        case 4:
                            Program.odpowiedź = 4;
                            Połączenie.stan = "dostępny";
                            break;
                        case 5:
                            Program.odpowiedź = 5;
                            break;
                        case 11:
                            Console.WriteLine("Telefon docelowy wybrał usługę transmisji danych");
                            Połączenie.rodzaj_usług = 'd';
                            wyślij_komunikat(stwórz_komunikat("ACK", Połączenie.rozmówca.nick));
                            break;
                        case 12:
                            Console.WriteLine("Telefon docelowy wybrał usługę transmisji audio");
                            Połączenie.rodzaj_usług = 'a';
                            wyślij_komunikat(stwórz_komunikat("ACK", Połączenie.rozmówca.nick));
                            break;
                        case 13:
                            Console.WriteLine("Telefon docelowy wybrał usługę transmisji wideo");
                            Połączenie.rodzaj_usług = 'w';
                            wyślij_komunikat(stwórz_komunikat("ACK", Połączenie.rozmówca.nick));
                            break;
                        case 21:
                            Console.WriteLine("Nastąpiło wyjątkowe rozłączenie telefonu z centralą!");
                            if (Połączenie.stan != "dostępny")
                            {
                                wyślij_komunikat(stwórz_komunikat("DISCONNECT", Połączenie.rozmówca.nick));
                            }
                            Program.telefon.Close();
                            break;
                        case 22:
                            Console.WriteLine("Rozmówca rozłączył się");
                            Połączenie.stan = "dostępny";
                            Połączenie.rodzaj_usług = '-';
                            Połączenie.port = 0;
                            Połączenie.blokada_wysłania_odpowiedzi = false;
                            break;
                        case 23:
                            if (Połączenie.stan == "dostępny")
                            {
                                Połączenie.stan = "łączenie";

                                Console.WriteLine("Użytkownik " + komunikat.nadawca.nick + " dzwoni.\nOdebrać?\n [t] tak\n [n] nie");

                                while (Połączenie.stan == "łączenie")
                                {
                                    //oczekiwanie aż użytkownik zaakceptuje lub odrzuci połączenie
                                    //odbywa się to w konsoli poprzez komendę działającą tylko przy łączeniu
                                    //komenda ta zmienia wartość stanu (klasa połączenie)
                                }
                                
                                if (Połączenie.stan == "zaakceptowano")
                                {
                                    Połączenie.stan = "łączenie";
                                    Połączenie.rozmówca = new User();
                                    Połączenie.rozmówca.nick = komunikat.nadawca.nick;
                                    Połączenie.rozmówca.mail = komunikat.nadawca.nick + "@domena.pl";
                                    Komunikat odpowiedź = stwórz_komunikat("ACK", komunikat.nadawca.nick);
                                    wyślij_komunikat(odpowiedź);
                                }
                                else if (Połączenie.stan == "odrzucono")
                                {
                                    Połączenie.stan = "dostępny";
                                    Komunikat odpowiedź = stwórz_komunikat("REJECT", komunikat.nadawca.nick);
                                    wyślij_komunikat(odpowiedź);
                                }
                                else
                                {
                                    Console.WriteLine("Nieobsługiwany stan " + Połączenie.stan);
                                }
                            }
                            else
                            {
                                Komunikat odpowiedź = stwórz_komunikat("BUSY", komunikat.nadawca.nick);
                                wyślij_komunikat(odpowiedź);
                            }
                            break;
                        case 31:
                            string kodekdanych = potnij_kodeki(komunikat.cseq.polecenie);
                            if (Połączenie.blokada_wysłania_odpowiedzi == false)
                            {
                                wyślij_komunikat(stwórz_komunikat("FORMATDANYCH;" + kodekdanych, Połączenie.rozmówca.nick));
                                Połączenie.zapisz_dane(kodekdanych);
                                Połączenie.blokada_wysłania_odpowiedzi = true;
                                Połączenie.stan = "zajęty";
                                Thread.Sleep(100); //czekam 100ms aby jakiś inny wątek nie zapisał konsoli po poniższym napisie; poniższy napis ma być ostatni
                                Console.WriteLine("Nawiązano połączenie!");
                            }
                            else
                            {
                                Połączenie.zapisz_dane(kodekdanych);
                                Połączenie.blokada_wysłania_odpowiedzi = false;
                            }
                            break;
                        case 32:
                            string kodekaudio = potnij_kodeki(komunikat.cseq.polecenie);
                            if (Połączenie.blokada_wysłania_odpowiedzi == false)
                            {
                                wyślij_komunikat(stwórz_komunikat("FORMATAUDIO;" + kodekaudio, Połączenie.rozmówca.nick));
                                Połączenie.zapisz_audio(kodekaudio);
                                Połączenie.blokada_wysłania_odpowiedzi = true;
                                Połączenie.stan = "zajęty";
                                Thread.Sleep(100); //czekam 100ms aby jakiś inny wątek nie zapisał konsoli po poniższym napisie; poniższy napis ma być ostatni
                                Console.WriteLine("Nawiązano połączenie!");
                            }
                            else
                            {
                                Połączenie.zapisz_dane(kodekaudio);
                                Połączenie.blokada_wysłania_odpowiedzi = false;
                            }
                            break;
                        case 33:
                            string kodekwideo = potnij_kodeki(komunikat.cseq.polecenie);
                            if (Połączenie.blokada_wysłania_odpowiedzi == false)
                            {
                                wyślij_komunikat(stwórz_komunikat("FORMATWIDEO;" + kodekwideo, Połączenie.rozmówca.nick));
                                Połączenie.zapisz_wideo(kodekwideo);
                                Połączenie.blokada_wysłania_odpowiedzi = true;
                                Połączenie.stan = "zajęty";
                                Thread.Sleep(100); //czekam 100ms aby jakiś inny wątek nie zapisał konsoli po poniższym napisie; poniższy napis ma być ostatni
                                Console.WriteLine("Nawiązano połączenie!");
                            }
                            else
                            {
                                Połączenie.zapisz_dane(kodekwideo);
                                Połączenie.blokada_wysłania_odpowiedzi = false;
                            }
                            break;
                        case 41:
                            string port = komunikat.cseq.polecenie.Substring(komunikat.cseq.polecenie.IndexOf(';') + 1);
                            Połączenie.port = Convert.ToInt32(port);
                            Console.WriteLine("Port dla wybranego typu usługi to: {0}", port);
                            wyślij_komunikat(stwórz_komunikat("ACK", Połączenie.rozmówca.nick));
                            break;
                        default:
                            Console.WriteLine("Nie można obsłużyć komunikatu dla telefonu o treści " + komunikat.cseq.polecenie + ".");
                            break;
                    }
                }
            }
        }
    }
}