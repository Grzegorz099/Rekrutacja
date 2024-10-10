using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(Zadanie1), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class Zadanie1
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class TemplateWorkerParametry : ContextBase
        {
            [Caption("Zmienna A")]
            public int A { get; set; }

            [Caption("Zmienna B")]
            public int B { get; set; }

            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            public TemplateWorkerParametry(Context context) : base(context)
            {
                this.DataObliczen = Date.Today;
            }

            [Caption("Operacja")]
            [Required]
            public string Operacja { get; set; }

        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public TemplateWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public object WykonajAkcje()
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu


            var pracownicy = this.Cx[typeof(Pracownik[])] as Pracownik[];

            //Modyfikacja danych
            //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
            using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                int rekordy = 0;
                //Otwieramy Transaction aby można było edytować obiekt z sesji
                using (ITransaction trans = nowaSesja.Logout(true))
                {
                    foreach (var pracownik in pracownicy)
                    {
                        //Pobieramy obiekt z Nowo utworzonej sesji
                        var pracownikZSesja = nowaSesja.Get(pracownik);

                        double wynik = 0;
                        switch (Parametry.Operacja)
                        {
                            case "+":
                                wynik = Parametry.A + Parametry.B;
                                break;
                            case "-":
                                wynik = Parametry.A - Parametry.B;
                                break;
                            case "*":
                                wynik = Parametry.A * Parametry.B;
                                break;
                            case "/":
                                if (Parametry.B == 0)
                                    return "Niedozwolona operacja!";
                                wynik = (double)(Parametry.A / Parametry.B);
                                break;
                            default:
                                return "Niedozwolona operacja!";
                        }

                        //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                        pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = wynik;
                        rekordy++;

                    }
                    //Zatwierdzamy zmiany wykonane w sesji
                    trans.CommitUI();
                }
                //Zapisujemy zmiany
                nowaSesja.Save();
                return "Akcja wykonana pomyślnie. (zmodyfikowano rekordów " + rekordy + " )";
            }
        }
    }
}