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
        // --- Importações do Mouse (Win32 API) ---
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        // Constantes de Ação do Mouse
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        // Diretório Base (ajustado para rodar fora do bin/Debug)
        static readonly string BaseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        static void Main(string[] args)
        {
            Console.WriteLine("=== BOT BALL X PIT - V5 COMPLETA (Colheita + Personagem + Mapa) ===");
            Console.WriteLine($"Diretório base: {BaseDir}");
            Console.WriteLine("Pressione Ctrl+C para parar.");
            Console.WriteLine("------------------------------------------------");

            while (true)
            {
                try
                {
                    ExecutarRotinaCompleta();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO CRÍTICO NA MAIN] {ex.Message}");
                }

                // Pausa entre ciclos completos
                Thread.Sleep(2000);
            }
        }

        static void ExecutarRotinaCompleta()
        {
            // ==============================================================================
            // PASSO 1: COLHEITA
            // ==============================================================================
            // Verifica se o botão de colher existe (sem clicar ainda)
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

                Console.WriteLine(">> Acelerando (Segurando botão direito)...");
                SegurarBotaoDireito(locColher.X, locColher.Y, 10000); // 10 segundos

                Console.WriteLine(">> Aguardando Recompensa...");
                Thread.Sleep(2000);

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
            if (TentarEncontrarEClicar("botoes/botao_batalha.png", 3))
            {
                Console.WriteLine("Clicou em Batalha. Aguardando menu de personagens...");

                // Espera até 10 segundos pelo cabeçalho do menu de personagens
                // (Usando o nome corrigido 'headerpersonagens.png' sem underline, conforme seu arquivo)
                if (AguardarImagem("personagens/headerpersonagens.png", 10))
                {
                    Console.WriteLine("Menu de Personagens detectado!");

                    // Executa a lógica de montar o time
                    ExecutarSelecaoDeTime();

                    // Se a seleção de time deu certo e clicou em continuar, vamos para o MAPA
                    Console.WriteLine(">> Aguardando transição para tela de Mapas...");
                    Thread.Sleep(3000);

                    // Executa a lógica de escolher o mapa e iniciar o jogo
                    ExecutarSelecaoDeMapa();
                }
                else
                {
                    Console.WriteLine("[AVISO] Cabeçalho do menu de personagens não apareceu.");
                    Console.WriteLine("Dica: Verifique se o arquivo em 'Images/personagens/headerpersonagens.png' existe.");
                }
            }
        }

        static void ExecutarSelecaoDeTime()
        {
            Console.WriteLine("--- MONTANDO O TIME ---");

            // 1. Selecionar o RADICAL (Obrigatório)
            Console.WriteLine(">> Procurando Radical...");
            if (TentarEncontrarEClicar("personagens/radical.png", 5))
            {
                Thread.Sleep(1000);
                // Clica em SELECIONAR
                TentarEncontrarEClicar("botoes/botao_selecionar.png", 3);
                Console.WriteLine(">> Radical Selecionado.");
                Thread.Sleep(1500);
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

            // 3. Selecionar ALEATÓRIO
            Console.WriteLine(">> Escolhendo parceiro aleatório...");
            if (EscolherPersonagemAleatorio())
            {
                Thread.Sleep(1500);
                TentarEncontrarEClicar("botoes/botao_selecionar.png", 3);
                Thread.Sleep(1500);

                // 4. Ir para Mapas
                Console.WriteLine(">> Clicando em CONTINUAR (Indo para mapas)...");
                if (TentarEncontrarEClicar("botoes/botao_continuar.png", 5))
                {
                    // A função ExecutarSelecaoDeMapa será chamada na RotinaCompleta após isso
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Nenhum parceiro aleatório encontrado.");
            }
        }

        static void ExecutarSelecaoDeMapa()
        {
            Console.WriteLine("--- SELEÇÃO DE MAPA ---");

            MoverParaCantoEsquerdo();

            // 1. Validar se estamos no NOVO JOGO+
            // Se NÃO encontrar a imagem "Novo_Jogo_+", clica na setinha da direita
            if (LocalizarImagem("mapas/Novo_Jogo_+.png", 0.7) == Point.Empty)
            {
                Console.WriteLine(">> Não estamos no Novo Jogo+. Clicando na seta direita...");
                if (TentarEncontrarEClicar("botoes/seta_direita.png", 3))
                {
                    Thread.Sleep(2000); // Espera a animação de troca de aba
                }
                else
                {
                    Console.WriteLine("[ERRO] Seta direita não encontrada. Continuando mesmo assim...");
                }
            }
            else
            {
                Console.WriteLine(">> Já estamos no Novo Jogo+.");
            }

            // 2. Escolher e buscar mapa aleatório
            Console.WriteLine(">> Buscando mapa aleatório...");
            bool mapaEncontrado = false;
            int tentativasScroll = 0;
            const int MAX_SCROLLS = 15; // Limite para não ficar num loop infinito

            // Sorteia qual mapa queremos jogar
            string mapaAlvo = SortearMapaAlvo();
            if (string.IsNullOrEmpty(mapaAlvo))
            {
                Console.WriteLine("[ERRO] Não há mapas válidos na pasta Images/mapas.");
                return;
            }
            Console.WriteLine($">> O Bot quer jogar no mapa: {Path.GetFileNameWithoutExtension(mapaAlvo)}");

            while (!mapaEncontrado && tentativasScroll < MAX_SCROLLS)
            {
                // Tenta achar o mapa sorteado na tela atual
                if (TentarEncontrarEClicar($"mapas/{mapaAlvo}", 2))
                {
                    Console.WriteLine(">> Mapa encontrado e selecionado!");
                    mapaEncontrado = true;
                    Thread.Sleep(1000); // Espera seleção
                    break;
                }

                // Se não achou, verifica se chegou no fim da lista
                // (mapabloqueado ou vaziovasto indicam o final)
                if (LocalizarImagem("mapas/mapabloqueado.png", 0.7) != Point.Empty ||
                    LocalizarImagem("mapas/vaziovasto.png", 0.7) != Point.Empty)
                {
                    Console.WriteLine(">> Chegamos no fim da lista. Voltando ao topo...");
                    // Scrollzão pra cima (8 vezes)
                    for (int i = 0; i < 8; i++)
                    {
                        ScrollMouse(1000); // Valor positivo = Rolar para cima
                        Thread.Sleep(300);
                    }
                    tentativasScroll = 0; // Reseta contador para procurar desde o topo

                    // Sorteia outro mapa para garantir que não ficamos presos procurando um mapa que não existe
                    mapaAlvo = SortearMapaAlvo();
                    Console.WriteLine($">> Trocando alvo para: {Path.GetFileNameWithoutExtension(mapaAlvo)}");
                    continue;
                }

                // Scrollzão pra cima (10 vezes) para começar pelo primeiro mapa. O jogo começa com o scroll no meio da lista.
                for (int i = 0; i < 10; i++)
                {
                    ScrollMouse(1000); // Valor positivo = Rolar para cima
                    Thread.Sleep(300);
                }
                // Se não achou e não é o fim, desce a tela
                Console.WriteLine(">> Mapa não visível. Rolando para baixo...");
                ScrollMouse(-1000); // Valor negativo = Rolar para baixo
                Thread.Sleep(500); // Espera o scroll assentar
                tentativasScroll++;
            }

            if (mapaEncontrado)
            {
                Console.WriteLine(">> Mapa selecionado. Clicando em JOGAR...");

                // --- ALTERAÇÃO AQUI ---
                // Clica no botão Jogar para efetivamente começar a partida
                if (TentarEncontrarEClicar("botoes/botao_jogar.png", 5))
                {
                    Console.WriteLine("--- PARTIDA INICIADA ---");
                    // Agora o bot dorme enquanto você joga, até implementarmos a detecção de Vitória/Derrota
                    Thread.Sleep(10000);
                }
                else
                {
                    Console.WriteLine("[ERRO] Botão JOGAR não encontrado após selecionar o mapa.");
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Não consegui selecionar o mapa desejado após várias tentativas.");
            }
        }

        // --- FUNÇÕES DE LÓGICA ALEATÓRIA ---

        static string SortearMapaAlvo()
        {
            try
            {
                string pastaMapas = Path.Combine(BaseDir, "Images", "mapas");
                if (!Directory.Exists(pastaMapas)) return "";

                var arquivos = Directory.GetFiles(pastaMapas, "*.*")
                                       .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"))
                                       .ToList();

                Random rng = new Random();

                // Filtra arquivos que NÃO são mapas jogáveis
                var mapasValidos = arquivos.Where(f =>
                {
                    string nome = Path.GetFileName(f).ToLower();
                    // Ignora o título, setas e os mapas de fim de lista
                    return !nome.Contains("novo_jogo") &&
                           !nome.Contains("bloqueado") &&
                           !nome.Contains("vaziovasto") &&
                           !nome.Contains("seta");
                }).ToList();

                if (mapasValidos.Count > 0)
                {
                    // Retorna apenas o nome do arquivo (ex: "florestafungica.png")
                    return Path.GetFileName(mapasValidos[rng.Next(mapasValidos.Count)]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao sortear mapa: {ex.Message}");
            }
            return "";
        }

        static bool EscolherPersonagemAleatorio()
        {
            try
            {
                string pastaPersonagens = Path.Combine(BaseDir, "Images", "personagens"); // Pasta com 'n'
                if (!Directory.Exists(pastaPersonagens)) return false;

                var arquivos = Directory.GetFiles(pastaPersonagens, "*.*")
                                       .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"))
                                       .ToList();

                Random rng = new Random();
                var arquivosEmbaralhados = arquivos.OrderBy(a => rng.Next()).ToList();

                foreach (var arquivo in arquivosEmbaralhados)
                {
                    string nomeArquivo = Path.GetFileName(arquivo);

                    // Ignora arquivos de controle (header, menu, radical)
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

        // --- FUNÇÕES UTILITÁRIAS (Core do Bot) ---

        static bool TentarEncontrarEClicar(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                Point pos = LocalizarImagem(caminhoRelativo, 0.7);
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

        static bool AguardarImagem(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                if (LocalizarImagem(caminhoRelativo, 0.7) != Point.Empty)
                    return true;

                Console.Write("."); // Feedback visual de espera
                Thread.Sleep(1000);
                tentativas++;
            }
            Console.WriteLine();
            return false;
        }

        static Point LocalizarImagem(string caminhoRelativo, double confiancaMinima)
        {
            // Corrige barras invertidas/normais para o Windows
            caminhoRelativo = caminhoRelativo.Replace('/', Path.DirectorySeparatorChar)
                                             .Replace('\\', Path.DirectorySeparatorChar);

            string caminhoCompleto = Path.Combine(BaseDir, "Images", caminhoRelativo);

            if (!File.Exists(caminhoCompleto))
            {
                // Tenta achar na raiz da pasta Images caso o subdiretório falhe
                string nomeArquivo = Path.GetFileName(caminhoCompleto);
                string caminhoAlternativo = Path.Combine(BaseDir, "Images", nomeArquivo);

                if (File.Exists(caminhoAlternativo))
                    caminhoCompleto = caminhoAlternativo;
                else
                {
                    Console.WriteLine($"[DEBUG] Arquivo não encontrado: {caminhoCompleto}");
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

                // Converte para 3 canais (BGR) se a tela tiver Alpha
                if (matTelaBruta.Channels() == 4)
                    Cv2.CvtColor(matTelaBruta, matTela3Canais, ColorConversionCodes.BGRA2BGR);
                else
                    matTelaBruta.CopyTo(matTela3Canais);

                // Evita crash se a imagem for maior que a tela
                if (matTela3Canais.Width < matTemplate.Width || matTela3Canais.Height < matTemplate.Height)
                    return Point.Empty;

                Cv2.MatchTemplate(matTela3Canais, matTemplate, resultado, TemplateMatchModes.CCoeffNormed);

                double minVal, maxVal;
                OpenCvSharp.Point minLoc, maxLoc;
                Cv2.MinMaxLoc(resultado, out minVal, out maxVal, out minLoc, out maxLoc);

                // Feedback para ajudar a ajustar recortes
                if (maxVal > 0.2 && maxVal < confiancaMinima)
                {
                    // Descomente a linha abaixo se quiser ver "quase acertos"
                    // Console.WriteLine($"[DEBUG] '{Path.GetFileName(caminhoRelativo)}': {maxVal:P0} (Min: {confiancaMinima:P0})");
                }

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

        static void ScrollMouse(int quantidade)
        {
            // Move o mouse para o centro da tela para o scroll ser capturado pelo jogo
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            SetCursorPos(bounds.Width / 2, bounds.Height / 2);
            Thread.Sleep(100);

            // Evento de roda do mouse (dwData positivo = Cima, negativo = Baixo)
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, quantidade, 0);
        }

        static void MoverParaCantoEsquerdo()
        {
            Thread.Sleep(500);
            Console.WriteLine(">> Movendo mouse para o canto (teste)...");
            SetCursorPos(0, 0);
            Thread.Sleep(500); // Espera meio segundo pra você ver que ele foi pra lá
        }
    }
}