using Google.GenAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Gemini
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
    public class GeminiLlMConfig
    {
        private GenerateContentConfig PostGenerationConfig { get; set; }
        private GenerateContentConfig AskGenerationConfig { get; set; }
        private GenerateImagesConfig ImageGenerationConfig { get; set; }
        private GenerateContentConfig UserMimicGenerationConfig { get; set; }

        public GeminiLlMConfig()
        {
            PostGenerationConfig = new GenerateContentConfig
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
                                    - post musi mieć maksymalnie 2000 znaków, w zależności od potrzeb może być krótszy lub dłuższy, ale nie przekraczać limitu
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


            AskGenerationConfig = new GenerateContentConfig
            {
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                MaxOutputTokens = 2000,
                CandidateCount = 1,
                SystemInstruction = new Content
                {
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text = @"
                                Jesteś ekspertem ds. social media i marketingu cyfrowego.
                                
                                Twoją rolą jest pomaganie użytkownikowi w:
                                - tworzeniu pomysłów na posty
                                - planowaniu contentu
                                - pisaniu i poprawianiu treści marketingowych
                                - sugerowaniu hashtagów
                                - doradzaniu strategii publikacji w social media
                                
                                Odpowiadasz wyłącznie w języku polskim.
                                
                                Styl odpowiedzi:
                                - konkretny i praktyczny
                                - zwięzły, ale wartościowy
                                - możesz używać emoji
                                
                                Zasady:
                                - NIE dodawaj zbędnych wyjaśnień modelowych
                                - NIE wychodź poza temat social media i marketingu
                                - jeśli prosi o pomysły → generujesz listę pomysłów
                                - jeśli prosi o strategie → dajesz rekomendacje i wskazówki
                            "
                        }
                    }
                }
            };


            ImageGenerationConfig = new GenerateImagesConfig
            {
                NumberOfImages = 1,
                AspectRatio = "1:1",
                OutputMimeType = "image/png",
            };

            UserMimicGenerationConfig = new GenerateContentConfig
            {
                Temperature = 0.3f,
                TopP = 0.9f,
                TopK = 40,
                MaxOutputTokens = 2500,
                CandidateCount = 1,
                SystemInstruction = new Content
                {
                    Parts = new List<Part>
                {
                new Part
                {
                    Text = @"
                        Jesteś ekspertem od analizy stylu pisania.
                        
                        Otrzymasz jedną lub więcej próbek tekstu użytkownika.
                        
                        Twoim jedynym zadaniem jest wygenerowanie instrukcji SYSTEMOWEJ dla innego modelu językowego.
                        
                        Instrukcja ma opisywać WYŁĄCZNIE sposób pisania autora, a nie treść jego wypowiedzi.
                        
                        Zwróć uwagę między innymi na:
                        - ton wypowiedzi,
                        - poziom formalności,
                        - długość i budowę zdań,
                        - rytm tekstu,
                        - dobór słownictwa,
                        - charakterystyczne zwroty,
                        - sposób argumentacji,
                        - sposób rozpoczynania i kończenia tekstów,
                        - interpunkcję,
                        - używanie emoji,
                        - używanie pytań retorycznych,
                        - stosowanie CTA,
                        - sposób prowadzenia narracji,
                        - poziom energii wypowiedzi,
                        - elementy, których należy unikać.
                        
                        Wygenerowana instrukcja ma być napisana bezpośrednio do modelu językowego.
                        
                        Nie opisuj autora.
                        Nie oceniaj jakości tekstu.
                        Nie twórz podsumowania.
                        Nie zwracaj JSON.
                        Nie zwracaj Markdown.
                        Nie używaj nagłówków.
                        
                        Wynik ma być gotowym promptem systemowym, który można wkleić jako SystemInstruction do kolejnego modelu.
                        
                        Instrukcja powinna zawierać wyłącznie polecenia typu:
                        - Pisz...
                        - Zachowuj...
                        - Unikaj...
                        - Stosuj...
                        - Nie używaj...
                        - Naśladuj...
                        
                        Jeżeli próbka jest niewystarczająca do określenia jakiejś cechy, pomiń ją zamiast zgadywać.
                        
                        Nie wspominaj o analizie ani o użytkowniku. Zwróć wyłącznie gotową instrukcję."
                        }
                    }
                }
            };

        }

        public GenerateContentConfig GetPostGenerationConfig()
        {
            return PostGenerationConfig;
        }

        public GenerateContentConfig GetAskGenerationConfig()
        {
            return AskGenerationConfig;

        }

        public GenerateImagesConfig GetGenerateImagesConfig()
        {
            return ImageGenerationConfig;
        }

        public GenerateContentConfig GetUserMimicGenerationConfig()
        {
            return UserMimicGenerationConfig;
        }

    }
}
