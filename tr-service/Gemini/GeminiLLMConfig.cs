using Google.GenAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_service.Gemini
{

    /// <summary>
    /// Konfiguracja modelu Gemini do generowania postów na social media.
    /// 
    /// Klasa ustawia parametry generowania treści oraz instrukcję systemową,
    /// która ogranicza model wyłącznie do tworzenia gotowych postów w języku polskim.
    /// 
    /// Ustawienia:
    /// - Temperature (0.8): umiarkowana kreatywność (bardziej angażujące treści)
    /// - TopP (0.9): naturalna różnorodność językowa
    /// - TopK (40): kontrola wyboru tokenów
    /// - MaxOutputTokens (200): limit długości posta
    /// - CandidateCount (1): jedna odpowiedź (brak wariantów)
    /// 
    /// Instrukcja systemowa wymusza:
    /// - brak dodatkowych komentarzy i wyjaśnień
    /// - wyłącznie język polski
    /// - styl dopasowany do social media (emoji, hashtagi)
    /// - gotowy post do publikacji
    /// </summary>
    public class GeminiLLMConfig
    {
        private GenerateContentConfig Config {  get; set; }

        public GeminiLLMConfig()
        {
            Config = new GenerateContentConfig
            {
                Temperature = 0.8f,
                TopP = 0.9f,
                TopK = 40,
                MaxOutputTokens = 5000,
                CandidateCount = 1,

                SystemInstruction = new Content
                {
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text = @"
                                    Jesteś profesjonalnym copywriterem social media.
                                    
                                    Tworzysz wyłącznie gotowe posty w języku polskim.
                                    
                                    Wymagania:
                                    - post musi mieć 3–6 zdań lub kilka krótkich akapitów
                                    - pierwszy akapit = hook (przyciągający uwagę)
                                    - środek = rozwinięcie
                                    - końcówka = podsumowanie lub CTA
                                    - możesz używać emoji i hashtagów
                                    - NIE generuj krótkich jednozdaniowych postów
                                    - NIE dodawaj komentarzy ani wyjaśnień
                                    
                                    Odpowiadasz wyłącznie treścią posta. Na zadane pytania nie odpowiadasz, a
                                    zwracasz informacje o nieprawidłowym prompcie i prosisz o nowy.
                                   "
                        }
                    }
                },
            };
        }

        /// <summary>
        /// Zwraca skonfigurowany obiekt GenerateContentConfig.
        /// </summary>
        public GenerateContentConfig GetConfig()
        {
            return Config;
        }

    }
}
