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
using MySql.Data.MySqlClient;

namespace Beadando_Szenzorhalozat
{

    class Sensorok
    {
        //privát adattagok az egységbezárás és az adatok védelme miatt - Ádám
        private int homerseklet, paratartalom, folyoszint, tartalyszint, allapot, azon;
        //hogy elérje a többi osztály és lehessen dolgozni velük, tulajdonság függvények létrehozása - Ádám
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
        //konstruktor - Ádám
        public Sensorok(int azon, int homerseklet, int paratartalom, int folyoszint, int tartalyszint)
        {
            Azon = azon;
            Homerseklet = homerseklet;
            Paratartalom = paratartalom;
            Folyoszint = folyoszint;
            Tartalyszint = tartalyszint;
        }
        public override string ToString() //hogy kiírja a konkrét mérési eredményeket, az ellenőrző kiíratás során - Ádám
        {
            return $"Azonosító: {Azon}, Hőmérséklet: {Homerseklet}°C, Páratartalom: {Paratartalom}%, Folyószint: {Folyoszint}m, Tartályszint: {Tartalyszint}cm";
        }
    }
    internal class Program
    {
        
        static List<SzenzorLibrary> szenzorlist = new List<SzenzorLibrary>();

        public delegate void JSONWriteHandler(EventArgs e); //delegate az eseménykezeléshez - Ádám
        public static JSONWriteHandler JSON_FILE; //delegate példányosítása - Ádám
        static void FileWrittenHandler(EventArgs e)
        {
            Console.WriteLine("A fájl sikeresen ki lett írva!");
        }//az eseményt kezelő metódus - Ádám
        static List<Sensorok> list = new List<Sensorok>();
        static void Results()
        {
            try
            {
                string filePath = "sensor_adatok.txt"; // Szöveges fájl elérési útja - Ádám
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath); // Beolvassa a fájl sorait - Ádám
                    foreach (var line in lines)
                    {
                        var parts = line.Split(' '); // Szóközök mentén szétválasztja az adatokat - Ádám
                        if (parts.Length == 5)
                        {
                            // Adatok konvertálása és objektum hozzáadása a listához - Ádám
                            int azon = int.Parse(parts[0]);
                            int homerseklet = int.Parse(parts[1]);
                            int paratartalom = int.Parse(parts[2]);
                            int folyoszint = int.Parse(parts[3]);
                            int tartalyszint = int.Parse(parts[4]);

                            list.Add(new Sensorok(azon, homerseklet, paratartalom, folyoszint, tartalyszint)); //példányosítás - Ádám

                            //Adatok behelyezése az adatbázisba -TZS
                            SzenzorLibrary sz = new SzenzorLibrary
                            {
                                azon = azon,
                                hom = homerseklet,
                                para = paratartalom,
                                folyoszint = folyoszint,
                                tartalyszint = tartalyszint
                            };

                            Beszur(null, sz);
                        }
                    }
                    /*foreach (var sensorok in list) //ellenőrzés képpen - Ádám
                    {
                        Console.WriteLine(sensorok);
                    }*/
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Váratlan hiba törént...\n"+e);
            }
        }//fájl-ból beolvasás - Ádám
        static void JSON()
        {
            string json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
            try
            {
                StreamWriter sw = new StreamWriter("json_adatok.txt"); //mérési adatok => JSON fájl - Ádám
                sw.WriteLine(json);
                sw.Flush();
                sw.Close();
                // Ha sikerült a fájl írása, aktiváljuk az eseményt a delegált hívásával - Ádám
                JSON_FILE?.Invoke(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a fájl írása során: {ex.Message}");
            }
        } //JSON fájl - Ádám
        static void LINQ_1()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Red; //design - Ádám
                Console.WriteLine("Az alábbi órákban narancssárga riasztás volt érvényben");
                Console.ResetColor();
                var result = from t in list
                             where t.Homerseklet > 35
                             select t;
                foreach (var t in result)
                    Console.WriteLine($"Óra: {t.Azon} Hőmérséklet: {t.Homerseklet}");
            }
            catch (Exception)
            {

                Console.WriteLine("Hiba történt a lekérdezés során");
            }
        } //1. linq lekérdezés - TZS
        static void LINQ_2()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Red; //extra design - Ádám
                Console.WriteLine("Az alábbi órákban magas volt a páratartalom:");
                Console.ResetColor();
                var result = from p in list
                             where 60 <= p.Paratartalom //feltétel - TZS
                             select p;
                foreach (var p in result) //a lekérdezett adatok kiíratása konzolra - TZS
                    Console.WriteLine($"Óra: {p.Azon} Hőmérséklet: {p.Paratartalom}");
            }
            catch (Exception)
            {
                Console.WriteLine("Hiba történt a lekérdezés során");
            }
        } //2. linq lekérdezés - TZS
        static void LINQ_3() //3. linq lekérdezés - TZS
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Red; //kis design utólag - Ádám
                Console.WriteLine("Az alábbi órákban nagyon magas szárazság volt");
                Console.ResetColor();
                var result = from s in list
                             where 30 < s.Homerseklet && 40 > s.Paratartalom //feltétel - TZS
                             select s;
                foreach (var s in result) //lekérdezett adatok kiíratása konzolra - TZS
                    Console.WriteLine($"Óra: {s.Azon} Hőmérséklet: {s.Homerseklet} Páratartalom: {s.Paratartalom}");
            }
            catch (Exception)
            {
                Console.WriteLine("Hiba történt a lekérdezés során");
            }
        }
        static void Main(string[] args)
        {
            Results();
            byte valasztas; //ez a változó a menürendszer kulcs eleme, ez lesz a fh. választsása -Ádám
            Console.WriteLine("Kérem válasszon az alábbi lehetőségek közül!\n\t1. A mérési adatok kiíratása egy JSON fájlba\n\t2. Az órák ahol narancssárga riasztás volt érvényben\n\t3. Az órák, ahol minimum 60% volt a páratartalom\n\t4. Órák, amikor a legszárazabb időjárás volt\n\t0. Kilépés"); 
            do
            {
                do //ellenőrzött beolvasás - Ádám
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\nAdja meg a választását az alábbi lehetőségek közül:");
                    Console.ResetColor();
                } while (!byte.TryParse(Console.ReadLine(), out valasztas));
                switch (valasztas) //switchekkel, menürendszer - Ádám
                {
                    case 0: //Kilépés - Ádám
                        break;
                    case 1:
                        JSON_FILE += FileWrittenHandler; //Eseményre való feliratkoztatás - Ádám
                        JSON(); //JSON meghívása - Ádám
                        break;
                    case 2:
                        //LINQ_1 meghívása - TZS
                        LINQ_1();
                        break;
                    case 3:
                        //LINQ_2 meghívása - TZS
                        LINQ_2();
                        break;
                    case 4:
                        //LINQ_3 meghívása - TZS
                        LINQ_3();
                        break;
                    default:
                        Console.WriteLine("Ilyen sorszámú lehetőség nincs!");
                        break;
                }
            } while (valasztas != 0);

        }

        public static void Adatbazis(int azon, int para, int hom, int folyoszint, int tartalyszint)//TZS
        {
            string kapcsolodas = "server=localhost;database=szenzorhalozat;user=root;password=root;"; //connection string -TZS
            using (var kapcsolo = new MySqlConnection(kapcsolodas))
            {
                kapcsolo.Open();
                string query = "INSERT INTO szenzorhalozat (azon, para, hom, folyoszint, tartalyszint) Values (@azon,@para,@hom,@folyoszint,@tartalyszint)"; //Ez a parancs lesz kiadva az adatbázisnak - TZS

                using (var parancsok = new MySqlCommand(query, kapcsolo))
                {
                    parancsok.Parameters.AddWithValue("@azon", azon);
                    parancsok.Parameters.AddWithValue("@para", para);
                    parancsok.Parameters.AddWithValue("@hom", hom);
                    parancsok.Parameters.AddWithValue("@folyoszint", folyoszint);
                    parancsok.Parameters.AddWithValue("@tartalyszint", tartalyszint);
                }
            }
        }

        public static void Beszur(object sender, SzenzorLibrary sz) //adatok adatbázisba mentése - TZS
        {
            szenzorlist.Add(sz);
            Adatbazis(sz.azon, sz.para, sz.hom, sz.folyoszint, sz.tartalyszint);
        }
    }
}