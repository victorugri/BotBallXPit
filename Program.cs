using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq; // Importante para listas e aleatoriedade
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
            Console.WriteLine("=== BOT BALL X PIT - V3 (Seleção de Personagens) ===");
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

            // Se a localização for diferente de Empty (0,0), significa que achou
            if (locColher != Point.Empty)
            {
                Console.WriteLine("--- COLHEITA DETECTADA ---");
                Console.WriteLine($"Coordenadas: {locColher}");

                // Agora sim iniciamos a sequência manual usando a coordenada que já temos

                Console.WriteLine(">> Clique 1 (Abrir)...");
                Clicar(locColher.X, locColher.Y);
                Thread.Sleep(3000); // Espera animação

                // Como o botão de colher fica no mesmo lugar para "lançar", usamos a mesma coord
                Console.WriteLine(">> Clique 2 (Lançar)...");
                Clicar(locColher.X, locColher.Y);
                Thread.Sleep(3000);

                Console.WriteLine(">> Acelerando (Botão Direito)...");
                SegurarBotaoDireito(locColher.X, locColher.Y, 10000); // 10 segundos

                Console.WriteLine(">> Aguardando Recompensa...");
                Thread.Sleep(2000);

                // Procura o botão "LEGAL"
                if (TentarEncontrarEClicar("botoes/botao_legal.png", 10))
                {
                    Console.WriteLine("Botão Legal clicado.");
                }
                else
                {
                    Console.WriteLine("[AVISO] Botão Legal não apareceu. Seguindo...");
                }

                Thread.Sleep(3000);
            }

            // ==============================================================================
            // PASSO 2: ENTRAR NA BATALHA
            // ==============================================================================
            // Tenta achar o botão de batalha
            if (TentarEncontrarEClicar("botoes/botao_batalha.png", 3))
            {
                Console.WriteLine("Clicou em Batalha. Aguardando menu...");
                Thread.Sleep(3000);

                // Verifica se o cabeçalho do menu apareceu
                if (LocalizarImagem("personages/header_personagens.png", 0.8) != Point.Empty)
                {
                    Console.WriteLine("Menu detectado!");
                    ExecutarSelecaoDeTime();
                }
                else
                {
                    Console.WriteLine("[AVISO] Cabeçalho do menu não encontrado.");
                }
            }
        }

        static void ExecutarSelecaoDeTime()
        {
            Console.WriteLine("--- MONTANDO O TIME ---");

            // 1. Selecionar o RADICAL (Obrigatório)
            Console.WriteLine(">> Procurando Radical...");
            if (TentarEncontrarEClicar("personages/radical.png", 5))
            {
                Thread.Sleep(1000);
                // Clica em SELECIONAR
                if (TentarEncontrarEClicar("botoes/selecionar.png", 3))
                {
                    Console.WriteLine(">> Radical Selecionado.");
                    Thread.Sleep(1500);
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Radical não encontrado! Abortando seleção.");
                return;
            }

            // 2. Clicar no slot VAZIO (Novo Parceiro)
            Console.WriteLine(">> Clicando no slot vazio...");
            if (!TentarEncontrarEClicar("botoes/novo_parceiro.png", 5))
            {
                Console.WriteLine("[ERRO] Slot 'Novo Parceiro' não encontrado.");
                return;
            }
            Thread.Sleep(1500);

            // 3. Selecionar ALEATÓRIO (Exceto Radical)
            Console.WriteLine(">> Escolhendo parceiro aleatório...");
            if (EscolherPersonagemAleatorio())
            {
                Thread.Sleep(1000);
                // Clica em SELECIONAR novamente
                TentarEncontrarEClicar("botoes/selecionar.png", 3);
                Thread.Sleep(1500);

                // 4. Iniciar Partida
                Console.WriteLine(">> Clicando em CONTINUAR...");
                if (TentarEncontrarEClicar("botoes/continuar.png", 5))
                {
                    Console.WriteLine("--- PARTIDA INICIADA ---");
                    // Delay longo para garantir que saiu da tela e o bot não tente clicar em nada
                    Thread.Sleep(10000);
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Nenhum parceiro aleatório visível encontrado.");
            }
        }

        static bool EscolherPersonagemAleatorio()
        {
            try
            {
                string pastaPersonagens = Path.Combine(BaseDir, "Images", "personages");

                // Pega todos os arquivos png/jpg da pasta
                var arquivos = Directory.GetFiles(pastaPersonagens, "*.*")
                                       .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"))
                                       .ToList();

                // Embaralha a lista (Shuffle)
                Random rng = new Random();
                var arquivosEmbaralhados = arquivos.OrderBy(a => rng.Next()).ToList();

                foreach (var arquivo in arquivosEmbaralhados)
                {
                    string nomeArquivo = Path.GetFileName(arquivo);

                    // Pula o Radical e o próprio print do menu se estiver lá
                    if (nomeArquivo.ToLower().Contains("radical") ||
                        nomeArquivo.ToLower().Contains("menu_personagens"))
                        continue;

                    // Tenta achar esse personagem na tela
                    // Usamos um caminho relativo para aproveitar a função LocalizarImagem
                    Point pos = LocalizarImagem($"personages/{nomeArquivo}", 0.85); // Confiança alta para distinguir bonecos parecidos

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

        // --- FUNÇÕES UTILITÁRIAS ---

        static bool TentarEncontrarEClicar(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                Point pos = LocalizarImagem(caminhoRelativo, 0.8);
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
            // Constrói o caminho completo baseado no BaseDir
            // Isso permite passar "botoes/botao.png" ou "personages/boneco.png"
            string caminhoCompleto = Path.Combine(BaseDir, "Images", caminhoRelativo);

            if (!File.Exists(caminhoCompleto))
            {
                // Fallback: Tenta achar direto na pasta Images se falhar
                string nomeArquivo = Path.GetFileName(caminhoCompleto);
                string caminhoAlternativo = Path.Combine(BaseDir, "Images", nomeArquivo);

                if (File.Exists(caminhoAlternativo))
                    caminhoCompleto = caminhoAlternativo;
                else
                    return Point.Empty;
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