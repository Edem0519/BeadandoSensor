using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Bead_2024;
using System.Runtime.InteropServices;
//using MySql.Data.MySqlClient;

namespace Beadando_Szenzorhalozat
{

    class Sensorok
    {
        //privát adattagok az egységbezárás és az adatok védelme miatt
        private int homerseklet, paratartalom, folyoszint, tartalyszint, allapot, azon;
        //hogy elérje a többi osztály és lehessen dolgozni velük, tulajdonság függvények létrehozása
        public int Homerseklet
        { get; set; }
        public int Paratartalom
        { get; set; }
        public int Folyoszint
        { get; set; }
        public int Tartalyszint
        { get; set; }
        public int Azon
        { get; set; }
        //konstruktor
        public Sensorok(int azon, int homerseklet, int paratartalom, int folyoszint, int tartalyszint)
        {
            Azon = azon;
            Homerseklet = homerseklet;
            Paratartalom = paratartalom;
            Folyoszint = folyoszint;
            Tartalyszint = tartalyszint;
        }
        public override string ToString() //hogy kiírja a konkrét mérési eredményeket, az ellenőrző kiíratás során
        {
            return $"Azonosító: {Azon}, Hőmérséklet: {Homerseklet}°C, Páratartalom: {Paratartalom}%, Folyószint: {Folyoszint}m, Tartályszint: {Tartalyszint}cm";
        }
    }
    internal class Program
    {
        
        static List<SzenzorLibrary> szenzorlist = new List<SzenzorLibrary>();

        public delegate void JSONWriteHandler(EventArgs e);
        public static JSONWriteHandler JSON_FILE;
        static void FileWrittenHandler(EventArgs e)
        {
            Console.WriteLine("A fájl sikeresen ki lett írva!");
        }//az eseményt kezelő metódus
        static List<Sensorok> list = new List<Sensorok>();
        static void Results()
        {
            string filePath = "adatok.txt"; // Szöveges fájl elérési útja
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath); // Beolvassa a fájl sorait
                foreach (var line in lines)
                {
                    var parts = line.Split(' '); // Szóközök mentén szétválasztja az adatokat
                    if (parts.Length == 5)
                    {
                        // Adatok konvertálása és objektum hozzáadása a listához
                        int azon = int.Parse(parts[0]);
                        int homerseklet = int.Parse(parts[1]);
                        int paratartalom = int.Parse(parts[2]);
                        int folyoszint = int.Parse(parts[3]);
                        int tartalyszint = int.Parse(parts[4]);

                        list.Add(new Sensorok(azon, homerseklet, paratartalom, folyoszint, tartalyszint));

                        //Adatok behelyezése az adatbázisba
                        /*SzenzorLibrary sz = new SzenzorLibrary
                        {
                            azon = azon,
                            hom = homerseklet,
                            para = paratartalom,
                            folyoszint = folyoszint,
                            tartalyszint = tartalyszint
                        };

                        Beszur(null, sz);*/
                    }
                }
                foreach (var sensorok in list) //ellenőrzés képpen
                {
                    Console.WriteLine(sensorok);
                }
            }
        }//fájl-ból beolvasás
        static void JSON()
        {
            string json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
            try
            {
                StreamWriter sw = new StreamWriter("json_adatok.txt"); //mérési adatok => JSON fájl
                sw.WriteLine(json);
                sw.Flush();
                sw.Close();
                // Ha sikerült a fájl írása, aktiváljuk az eseményt a delegált hívásával
                JSON_FILE?.Invoke(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a fájl írása során: {ex.Message}");
            }
        } //JSON fájl
        static void LINQ_1()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Az alábbi órákban narancssárga riasztás volt érvényben");
            Console.ResetColor();
            var result = from t in list
                         where t.Homerseklet > 35
                         select t;
            foreach (var t in result)
                Console.WriteLine($"Óra: {t.Azon} Hőmérséklet: {t.Homerseklet}");
        } //1. linq lekérdezés - ez rendben van...
        static void LINQ_2()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Az alábbi órákban magas volt a páratartalom:");
            Console.ResetColor();
            var result = from p in list
                         where 60 <= p.Paratartalom
                         select p;
            foreach (var p in result)
                Console.WriteLine($"Óra: {p.Azon} Hőmérséklet: {p.Paratartalom}");
        } //2. linq lekérdezés
        static void LINQ_3() //3. linq lekérdezés
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Az alábbi órákban nagyon magas szárazság volt");
            Console.ResetColor();
            var result = from s in list
                         where 30 < s.Homerseklet && 40 > s.Paratartalom
                         select s;
            foreach (var s in result)
                Console.WriteLine($"Óra: {s.Azon} Hőmérséklet: {s.Homerseklet} Páratartalom: {s.Paratartalom}");
        }
        static void Main(string[] args)
        {
            Results();
            byte valasztas; //ez a változó a menürendszer kulcs eleme, ez lesz a fh. választsása
            Console.WriteLine("Kérem válasszon az alábbi lehetőségek közül!\n\t1. A mérési adatok kiíratása egy JSON fájlba\n\t2. Az órák ahol narancssárga riasztás volt érvényben\n\t3. Az órák, ahol minimum 60% volt a páratartalom\n\t4. Órák, amikor a legszárazabb időjárás volt\n\t0. Kilépés"); //
            do
            {
                do //ellenőrzött beolvasás - ide esemény?
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\nAdja meg a választását az alábbi lehetőségek közül:");
                    Console.ResetColor();
                } while (!byte.TryParse(Console.ReadLine(), out valasztas));
                switch (valasztas) //switchekkel, menürendszer
                {
                    case 0: //Kilépés
                        break;
                    case 1:
                        JSON_FILE += FileWrittenHandler;
                        JSON(); //JSON meghívása
                        break;
                    case 2:
                        //LINQ_1 meghívása
                        LINQ_1();
                        break;
                    case 3:
                        //LINQ_2 meghívása
                        LINQ_2();
                        break;
                    case 4:
                        //LINQ_3 meghívása
                        LINQ_3();
                        break;
                    default:
                        Console.WriteLine("Ilyen sorszámú lehetőség nincs!");
                        break;
                }
            } while (valasztas != 0);

        }

        /*public static void Adatbazis(string azon, int para, int hom, int folyoszint, int tartalyszint)//TZS
        {
            string kapcsolodas = "server=localhost;database=szenzorhalozat;user=root;password=root;";
            using (var kapcsolo = new MySqlConnection(kapcsolodas))
            {
                kapcsolo.Open();
                string query = "INSERT INTO szenzorhalozat (azon, para, hom, folyoszint, tartalyszint) Values (@azon,@para,@hom,@folyoszint,@tartalyszint)";

                using (var parancsok = new MySqlCommand(query, kapcsolo))
                {
                    parancsok.Parameters.AddWithValue("@azon", azon);
                    parancsok.Parameters.AddWithValue("@para", para);
                    parancsok.Parameters.AddWithValue("@hom", hom);
                    parancsok.Parameters.AddWithValue("@folyoszint", folyoszint);
                    parancsok.Parameters.AddWithValue("@tartalyszint", tartalyszint);
                }
            }
        }*/
        //NEM JÓ A GIT!!!!
        //Most már lehet jó lesz a git
        /*public static void Beszur(object sender, SzenzorLibrary sz)
        {
            szenzorlist.Add(sz);
            Adatbazis(sz.azon, sz.para, sz.hom, sz.folyoszint, sz.tartalyszint);
        }*/
    }
}