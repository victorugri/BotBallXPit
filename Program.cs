using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using Point = System.Drawing.Point;

namespace BotBallXPit
{
    class Program
    {
        // --- Importações do Mouse ---
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        static readonly string BaseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        static void Main(string[] args)
        {
            Console.WriteLine("=== BOT BALL X PIT - V4 (Estável) ===");
            Console.WriteLine($"Diretório: {BaseDir}");

            while (true)
            {
                try
                {
                    ExecutarRotinaCompleta();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO CRÍTICO] {ex.Message}");
                }
                Thread.Sleep(2000);
            }
        }

        static void ExecutarRotinaCompleta()
        {
            // ==============================================================================
            // PASSO 1: COLHEITA
            // ==============================================================================
            // Tenta localizar a imagem (sem clicar ainda!)
            Point locColher = LocalizarImagem("botoes/botao_colher.png", 0.7);

            if (locColher != Point.Empty)
            {
                Console.WriteLine("--- COLHEITA DETECTADA ---");
                Console.WriteLine(">> Clique 1 (Abrir)...");
                Clicar(locColher.X, locColher.Y);
                Thread.Sleep(3000);

                Console.WriteLine(">> Clique 2 (Lançar)...");
                Clicar(locColher.X, locColher.Y);
                Thread.Sleep(3000);

                Console.WriteLine(">> Acelerando...");
                SegurarBotaoDireito(locColher.X, locColher.Y, 10000); // 10s

                Console.WriteLine(">> Aguardando Recompensa...");
                Thread.Sleep(2000);

                if (TentarEncontrarEClicar("botoes/botao_legal.png", 10))
                    Console.WriteLine("Botão Legal clicado.");

                Thread.Sleep(3000);
            }

            // ==============================================================================
            // PASSO 2: ENTRAR NA BATALHA
            // ==============================================================================
            if (TentarEncontrarEClicar("botoes/botao_batalha.png", 3))
            {
                Console.WriteLine("Clicou em Batalha. Aguardando menu...");

                // --- CORREÇÃO AQUI ---
                // Em vez de esperar fixo e olhar uma vez, usamos um loop de espera.
                // Isso resolve o problema se o menu demorar um pouco para carregar.
                if (AguardarImagem("personagens/headerpersonagens.png", 3))
                {
                    Console.WriteLine("Menu detectado!");
                    ExecutarSelecaoDeTime();
                }
                else
                {
                    Console.WriteLine("[AVISO] Cabeçalho do menu não apareceu após 10 segundos.");
                    Console.WriteLine("Dica: Verifique se o arquivo headerpersonagens.png é realmente um PNG.");
                }
            }
        }

        static void ExecutarSelecaoDeTime()
        {
            Console.WriteLine("--- MONTANDO O TIME ---");

            // 1. Selecionar o RADICAL
            Console.WriteLine(">> Procurando Radical...");
            if (TentarEncontrarEClicar("personagens/radical2.png", 5))
            {
                Thread.Sleep(1000);
                TentarEncontrarEClicar("botoes/selecionar.png", 3);
                Console.WriteLine(">> Radical Selecionado.");
                Thread.Sleep(1500);
            }
            else
            {
                Console.WriteLine("[ERRO] Radical não encontrado! (Verifique a imagem radical.png)");
                return;
            }

            // 2. Novo Parceiro
            Console.WriteLine(">> Clicando no slot vazio...");
            if (!TentarEncontrarEClicar("botoes/novo_parceiro.png", 5))
            {
                Console.WriteLine("[ERRO] Slot 'Novo Parceiro' não encontrado.");
                return;
            }
            Thread.Sleep(1500);

            // 3. Selecionar Aleatório
            Console.WriteLine(">> Escolhendo parceiro aleatório...");
            if (EscolherPersonagemAleatorio())
            {
                Thread.Sleep(1000);
                TentarEncontrarEClicar("botoes/selecionar.png", 3);
                Thread.Sleep(1500);

                Console.WriteLine(">> Clicando em CONTINUAR...");
                if (TentarEncontrarEClicar("botoes/continuar.png", 5))
                {
                    Console.WriteLine("--- PARTIDA INICIADA ---");
                    Thread.Sleep(10000);
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Nenhum parceiro aleatório encontrado.");
            }
        }

        // --- FUNÇÃO NOVA: Apenas espera algo aparecer na tela (sem clicar) ---
        static bool AguardarImagem(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                // Verifica se a imagem está na tela
                if (LocalizarImagem(caminhoRelativo, 0.7) != Point.Empty)
                    return true;

                Console.Write("."); // Feedback visual
                Thread.Sleep(1000);
                tentativas++;
            }
            Console.WriteLine();
            return false;
        }

        static bool EscolherPersonagemAleatorio()
        {
            try
            {
                string pastaPersonagens = Path.Combine(BaseDir, "Images", "personagens");
                if (!Directory.Exists(pastaPersonagens)) return false;

                var arquivos = Directory.GetFiles(pastaPersonagens, "*.*")
                                       .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"))
                                       .ToList();

                Random rng = new Random();
                var arquivosEmbaralhados = arquivos.OrderBy(a => rng.Next()).ToList();

                foreach (var arquivo in arquivosEmbaralhados)
                {
                    string nomeArquivo = Path.GetFileName(arquivo);

                    // Ignora arquivos de controle
                    if (nomeArquivo.ToLower().Contains("radical") ||
                        nomeArquivo.ToLower().Contains("header") ||
                        nomeArquivo.ToLower().Contains("menu"))
                        continue;

                    Point pos = LocalizarImagem($"personagens/{nomeArquivo}", 0.8);
                    if (pos != Point.Empty)
                    {
                        Console.WriteLine($"Parceiro escolhido: {nomeArquivo}");
                        Clicar(pos.X, pos.Y);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao escolher aleatório: {ex.Message}");
            }
            return false;
        }

        static bool TentarEncontrarEClicar(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                Point pos = LocalizarImagem(caminhoRelativo, 0.7); // Baixei levemente a confiança
                if (pos != Point.Empty)
                {
                    Clicar(pos.X, pos.Y);
                    return true;
                }
                Thread.Sleep(1000);
                tentativas++;
            }
            return false;
        }

        static Point LocalizarImagem(string caminhoRelativo, double confiancaMinima)
        {
            // CORREÇÃO: Garante que as barras estao certas para o Windows
            caminhoRelativo = caminhoRelativo.Replace('/', Path.DirectorySeparatorChar)
                                             .Replace('\\', Path.DirectorySeparatorChar);

            string caminhoCompleto = Path.Combine(BaseDir, "Images", caminhoRelativo);

            if (!File.Exists(caminhoCompleto))
            {
                string nomeArquivo = Path.GetFileName(caminhoCompleto);
                string caminhoAlternativo = Path.Combine(BaseDir, "Images", nomeArquivo);

                if (File.Exists(caminhoAlternativo))
                    caminhoCompleto = caminhoAlternativo;
                else
                {
                    // Debug para você saber se o arquivo não está sendo achado no disco
                    // Console.WriteLine($"[DEBUG] Arquivo não existe: {caminhoCompleto}");
                    return Point.Empty;
                }
            }

            using (Bitmap printScreen = CapturarTela())
            using (Mat matTelaBruta = BitmapConverter.ToMat(printScreen))
            using (Mat matTemplate = Cv2.ImRead(caminhoCompleto, ImreadModes.Color))
            using (Mat matTela3Canais = new Mat())
            using (Mat resultado = new Mat())
            {
                if (matTemplate.Empty()) return Point.Empty;

                if (matTelaBruta.Channels() == 4)
                    Cv2.CvtColor(matTelaBruta, matTela3Canais, ColorConversionCodes.BGRA2BGR);
                else
                    matTelaBruta.CopyTo(matTela3Canais);

                if (matTela3Canais.Width < matTemplate.Width || matTela3Canais.Height < matTemplate.Height)
                    return Point.Empty;

                Cv2.MatchTemplate(matTela3Canais, matTemplate, resultado, TemplateMatchModes.CCoeffNormed);

                double minVal, maxVal;
                OpenCvSharp.Point minLoc, maxLoc;
                Cv2.MinMaxLoc(resultado, out minVal, out maxVal, out minLoc, out maxLoc);

                if (maxVal >= confiancaMinima)
                {
                    return new Point(
                        maxLoc.X + (matTemplate.Width / 2),
                        maxLoc.Y + (matTemplate.Height / 2)
                    );
                }
            }
            return Point.Empty;
        }

        static Bitmap CapturarTela()
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        static void Clicar(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private static void SegurarBotaoDireito(int x, int y, int tempoMs)
        {
            SetCursorPos(x, y);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
            Thread.Sleep(tempoMs);
            mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }
    }
}