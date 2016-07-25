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
    class Komunikat
    {    //Pola nagłówka komunikatu zgodne z SIP
        public IPPort przez { get; set; } //VIA
        public User nadawca { set; get; } //From
        public User odbiorca { set; get; } //To
        public CallID callid { get; set; } 
        public CSeq cseq { get; set; }
        public Contact contact { get; set; }
        public string content_type { get; set; }
        public int content_length {get;set;}

        public string getstring() //Funkcja łącząca wszystkie pola w jednego stringa
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
            przez = new IPPort("0.0.0.0:0"); //defaultowa warość
            nadawca = new User();
            odbiorca = new User();
            callid = new CallID();
            cseq = new CSeq();
            contact = new Contact();
        }
    }

    class Komunikaty
    {
        static string potnij(ref string bufor) //Funkcja zwracająca fragment stringa do napotkania spacji, obcina oryginalnego stringa o kawałek który jest zwracany
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

        public static Komunikat odczytaj_komunikat(byte [] komunikat)
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
            komunikat.nadawca.nick = "centrala";
            komunikat.nadawca.mail = komunikat.nadawca.nick + "@domena.pl";
            komunikat.odbiorca.nick = nick;
            Telefon telefon = Program.ListaTelefonów.Find(bufor => bufor.user.nick == nick);
            if (telefon != null)
                komunikat.odbiorca.mail = telefon.user.mail;
            else
                komunikat.odbiorca.mail = "@domena.pl";
            komunikat.callid = new CallID();
            switch(komenda)
            {
                case "ACK":
                    komunikat.cseq.numer = 1;                
                    break;

                case "TERMINATE":
                    komunikat.cseq.numer = 21;
                    break;

                default:
                    komunikat.cseq.numer = 404;
                    break;
            }
            komunikat.cseq.polecenie = komenda;
            komunikat.contact.nick = komunikat.nadawca.nick;
            komunikat.contact.mail = komunikat.nadawca.mail;
            komunikat.content_type = "application/sdp";
            komunikat.content_length = komunikat.getstring().Length ;

            return komunikat;
        }

        public static bool wyślij_komunikat(Komunikat komunikat)
        {
            Telefon telefon = Program.ListaTelefonów.Find(buffor => buffor.user.nick == komunikat.odbiorca.nick); //wyszukiwanie telefonu o zadanym nicku na liście
            try
            {
                Console.WriteLine("Znalazłem telefon na liście " + telefon.user.nick);
            }
            catch
            {
                return false;
            }
            
            byte[] bufor = new byte [komunikat.getstring().Length]; //tworzenie tablicy bajtów o długości zadanego komunikatu
            bufor = Encoding.ASCII.GetBytes(komunikat.getstring()); //konwersja stringa do tablicy bajtów

            try
            {
                telefon.gniazdko.Send(bufor); //wysyłanie tablicy bajtów przez gniazdko do znalezionego telefonu
            }
            catch
            {
                return false;
            }

            Console.WriteLine("Wysłano komunikat " + komunikat.cseq.polecenie + " do telefonu " + telefon.user.nick);

            return true;
        }

        static public void obsłuż_komunikaty()
        {
            while(true)
            {
                if(Program.KolejkaKomunikatów.Count > 0)
                {
                    //zdejmuję z kolejki komunikatów komunikat, jeżeli tam jest
                    Komunikat komunikat = Program.KolejkaKomunikatów.Dequeue();

                    //znajduję adresata komunikatu
                    if (komunikat.odbiorca.nick == "centrala") //jeżeli odbiorcą jest centrala
                    {
                        
                        switch (komunikat.cseq.numer)
                        {
                            case 21: //obsłużenie komunikatu TERMINATE
                                Telefon telefon = Program.ListaTelefonów.Find(bufor => bufor.user.nick == komunikat.nadawca.nick);
                                Program.ListaTelefonów.Remove(telefon);
                                Console.WriteLine("Nastąpiło rozłączenie klienta!");
                                break;

                            case 6: //obsłużenie komunikatu CONNECT
                                Komunikat odpowiedź = stwórz_komunikat("ACK", komunikat.nadawca.nick);
                                wyślij_komunikat(odpowiedź);
                                Console.WriteLine("Zarejestrowano nowego klienta!");

                                break;

                            case 1: //Osługa komunikatu ACK
                                Console.WriteLine("Odebrano komunikat ACK");
                                break;

                            default:
                                Console.WriteLine("Nie można obsłużyć komunikatu dla centrali o treści " + komunikat.cseq.polecenie + ".");
                                break;
                        }
                    }
                    else //jeżeli odbiorcą jest inny telefon
                    {
                        if (wyślij_komunikat(komunikat) != true)
                        {
                            Komunikat odpowiedź = stwórz_komunikat("NOTFOUND", komunikat.nadawca.nick);
                            wyślij_komunikat(odpowiedź);
                            Console.WriteLine("Nie znaleziono wywoływanego użytkownika!");
                        }
                    }
                }
            }
        }
    }
}
