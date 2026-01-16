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
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        static readonly string BaseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        static string _mapaObrigatorio = "";

        static void Main(string[] args)
        {
            Console.WriteLine("=== BOT BALL X PIT - V9 CORREÇÃO DE LÓGICA CONC/INCONC ===");
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
                Thread.Sleep(2000);
            }
        }

        static void ExecutarRotinaCompleta()
        {
            _mapaObrigatorio = "";

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
                SegurarBotaoDireito(locColher.X, locColher.Y, 10000);

                Console.WriteLine(">> Aguardando Recompensa...");
                Thread.Sleep(2000);

                if (TentarEncontrarEClicar("botoes/botao_legal.png", 10))
                    Console.WriteLine("Botão Legal clicado.");

                Thread.Sleep(3000);
            }

            if (TentarEncontrarEClicar("botoes/botao_batalha.png", 3))
            {
                Console.WriteLine("Clicou em Batalha. Aguardando menu de personagens...");

                if (AguardarImagem("personagens/headerpersonagens.png", 10))
                {
                    Console.WriteLine("Menu de Personagens detectado!");
                    ExecutarSelecaoDeTime();

                    Console.WriteLine(">> Aguardando transição para tela de Mapas...");
                    Thread.Sleep(3000);

                    ExecutarSelecaoDeMapa();
                }
                else
                {
                    Console.WriteLine("[AVISO] Menu de personagens não apareceu.");
                }
            }
        }

        static void ExecutarSelecaoDeTime()
        {
            Console.WriteLine("--- MONTANDO O TIME ---");

            Console.WriteLine(">> Procurando Radical...");
            if (TentarEncontrarEClicar("personagens/radical2.png", 5))
            {
                Thread.Sleep(1000);
                TentarEncontrarEClicar("botoes/botao_selecionar.png", 3);
                Console.WriteLine(">> Radical Selecionado.");
                Thread.Sleep(1500);
            }
            else
            {
                Console.WriteLine("[ERRO] Radical não encontrado!");
                return;
            }

            Console.WriteLine(">> Clicando no slot vazio...");
            if (!TentarEncontrarEClicar("botoes/novo_parceiro.png", 5))
            {
                Console.WriteLine("[ERRO] Slot 'Novo Parceiro' não encontrado.");
                return;
            }
            Thread.Sleep(2000);

            Console.WriteLine(">> Analisando progresso dos personagens...");
            if (EscolherPersonagemPorProgresso())
            {
                Thread.Sleep(1000);
                TentarEncontrarEClicar("botoes/botao_selecionar.png", 3);
                Thread.Sleep(1500);

                Console.WriteLine(">> Clicando em CONTINUAR...");
                if (TentarEncontrarEClicar("botoes/botao_continuar.png", 5))
                {
                }
            }
            else
            {
                Console.WriteLine("[ERRO] Falha ao selecionar parceiro.");
            }
        }

        static void ExecutarSelecaoDeMapa()
        {
            Console.WriteLine("--- SELEÇÃO DE MAPA ---");

            if (LocalizarImagem("mapas/Novo_Jogo_+.png", 0.7) == Point.Empty)
            {
                Console.WriteLine(">> Não estamos no Novo Jogo+. Clicando na seta direita...");
                if (TentarEncontrarEClicar("botoes/seta_direita.png", 3))
                    Thread.Sleep(2000);
                else
                    Console.WriteLine("[ERRO] Seta direita não encontrada.");
            }
            else
            {
                Console.WriteLine(">> Já estamos no Novo Jogo+.");
            }

            Point pontoDeScroll = LocalizarImagem("botoes/scroll_lateral.png", 0.7);
            if (pontoDeScroll == Point.Empty)
            {
                var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                pontoDeScroll = new Point(bounds.Width / 2, bounds.Height / 2);
            }

            bool jogoIniciado = false;

            while (!jogoIniciado)
            {
                string mapaAlvo = "";

                if (!string.IsNullOrEmpty(_mapaObrigatorio))
                {
                    Console.WriteLine($">> [MODO PROGRESSÃO] O personagem precisa do mapa: {_mapaObrigatorio}");
                    mapaAlvo = _mapaObrigatorio + ".png";
                }
                else
                {
                    Console.WriteLine(">> [MODO ALEATÓRIO] Mapa livre ou progresso completo.");
                    mapaAlvo = SortearMapaAlvo();
                }

                if (string.IsNullOrEmpty(mapaAlvo)) { Console.WriteLine("[ERRO] Mapa inválido."); return; }

                bool mapaEncontrado = false;
                int tentativasScroll = 0;

                for (int i = 0; i < 5; i++) { ScrollMouse(5000, pontoDeScroll); Thread.Sleep(100); }

                while (!mapaEncontrado && tentativasScroll < 20)
                {
                    if (TentarEncontrarEClicar($"mapas/{mapaAlvo}", 1))
                    {
                        Console.WriteLine(">> Mapa selecionado!");
                        mapaEncontrado = true;
                        Thread.Sleep(1000);
                        break;
                    }

                    if (LocalizarImagem("mapas/vaziovasto.png", 0.7) != Point.Empty)
                    {
                        Console.WriteLine(">> Fim da lista de mapas.");

                        if (!string.IsNullOrEmpty(_mapaObrigatorio))
                        {
                            Console.WriteLine($"[ALERTA] O mapa obrigatório '{_mapaObrigatorio}' não foi encontrado.");
                            Console.WriteLine(">> Mudando para modo aleatório.");
                            _mapaObrigatorio = "";

                            for (int i = 0; i < 10; i++) { ScrollMouse(5000, pontoDeScroll); Thread.Sleep(150); }
                            mapaAlvo = SortearMapaAlvo();
                            break;
                        }

                        Console.WriteLine(">> Voltando ao topo...");
                        for (int i = 0; i < 10; i++) { ScrollMouse(5000, pontoDeScroll); Thread.Sleep(150); }
                        tentativasScroll = 0;
                        mapaAlvo = SortearMapaAlvo();
                        continue;
                    }

                    ScrollMouse(-300, pontoDeScroll);
                    Thread.Sleep(800);
                    tentativasScroll++;
                }

                if (!mapaEncontrado && string.IsNullOrEmpty(_mapaObrigatorio))
                {
                    continue;
                }

                if (mapaEncontrado)
                {
                    Console.WriteLine(">> Clicando em JOGAR...");
                    if (TentarEncontrarEClicar("botoes/botao_jogar.png", 3))
                    {
                        Console.WriteLine(">> Verificando aviso de 'Já Concluiu'...");
                        Thread.Sleep(2000);

                        if (LocalizarImagem("botoes/ja_concluiu.png", 0.7) != Point.Empty)
                        {
                            Console.WriteLine("[AVISO] Mapa já concluído! Recusando...");
                            if (TentarEncontrarEClicar("botoes/botao_nao.png", 3))
                            {
                                Thread.Sleep(2000);
                                _mapaObrigatorio = "";
                                continue;
                            }
                        }

                        jogoIniciado = true;
                        Console.WriteLine("--- PARTIDA INICIADA ---");
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            bool partidaAcabou = false;
            Console.WriteLine(">> Monitorando recompensas...");

            int ciclosSemAcharNada = 0;
            while (!partidaAcabou)
            {
                bool achouAlgo = false;

                if (TentarEncontrarEClicar("botoes/botao_uau.png", 1))
                {
                    Console.WriteLine(">> [UAU] Engrenagem!");
                    Thread.Sleep(5000);
                    achouAlgo = true;
                    continue;
                }

                if (TentarEncontrarEClicar("botoes/botao_legal.png", 1))
                {
                    Console.WriteLine(">> [LEGAL] Level Up!");
                    Thread.Sleep(5000);
                    achouAlgo = true;
                    continue;
                }

                if (TentarEncontrarEClicar("botoes/botao_legal.png", 1))
                {
                    Console.WriteLine(">> [LEGAL]... OUTRO Level Up!");
                    Thread.Sleep(5000);
                    achouAlgo = true;
                    continue;
                }

                Point posVoltar = LocalizarImagem("botoes/botao_voltarbase.png", 0.7);
                if (posVoltar != Point.Empty)
                {
                    Console.WriteLine("--- FIM DE JOGO ---");
                    Clicar(posVoltar.X, posVoltar.Y);
                    partidaAcabou = true;

                    Console.WriteLine(">> Voltando para a base...");
                    Thread.Sleep(8000);

                    TentarEncontrarEClicar("botoes/botao_legal.png", 3);
                    break;
                }

                if (!achouAlgo)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);

                    ciclosSemAcharNada++;
                    if (ciclosSemAcharNada % 30 == 0) Console.WriteLine();
                }
            }
        }

        static bool EscolherPersonagemPorProgresso()
        {
            string[] nomesMapas = new string[] {
                "patioosseo",
                "margensnevadas",
                "desertoliminar",
                "florestafungica",
                "savanasangrenta",
                "profundezasardentes",
                "portoescelestiais",
                "vaziovasto"
            };

            string[] arquivosConc = new string[] {
                "1patioosseoconc.png",
                "2margensnevadasconc.png",
                "3desertoliminarconc.png",
                "4florestafungicaconc.png",
                "5savanasangrentaconc.png",
                "6profundezasardentesconc.png",
                "7portoescelestiaisconc.png",
                "8vaziovastoconc.png"
            };

            string[] arquivosInconc = new string[] {
                "",
                "2margensnevadasINCONC.png",
                "3desertoliminarINCONC.png",
                "4florestafungicaINCONC.png",
                "5savanasangrentaINCONC.png",
                "6profundezasardentesINCONC.png",
                "7portoescelestiaisINCONC.png",
                "8vaziovastoINCONC.png"
            };

            try
            {
                string pastaPersonagens = Path.Combine(BaseDir, "Images", "personagens");
                var arquivosChar = Directory.GetFiles(pastaPersonagens, "*.png");

                foreach (var arquivoChar in arquivosChar)
                {
                    string nomeArquivo = Path.GetFileName(arquivoChar);
                    if (nomeArquivo.ToLower().Contains("radical") ||
                        nomeArquivo.ToLower().Contains("header") ||
                        nomeArquivo.ToLower().Contains("menu") ||
                        nomeArquivo.ToLower().Contains("nidificadora") ||
                        nomeArquivo.ToLower().Contains("arrependida")) continue;

                    Point pos = LocalizarImagem($"personagens/{nomeArquivo}", 0.85);
                    if (pos != Point.Empty)
                    {
                        Console.WriteLine($"Analizando personagem: {nomeArquivo}");
                        Clicar(pos.X, pos.Y);
                        Thread.Sleep(1500);

                        for (int i = 0; i < 8; i++)
                        {
                            // LÓGICA CORRIGIDA:
                            // 1. Verifica SE ESTÁ CONCLUÍDO (Check Verde).
                            //    Se SIM, continue para o próximo (não importa se parece incompleto).
                            bool estaCompleto = LocalizarImagem($"conclusoes/{arquivosConc[i]}", 0.95) != Point.Empty;
                            if (estaCompleto)
                            {
                                continue;
                            }

                            // 2. Se NÃO está concluído, verifica se está disponível (INCONC).
                            if (i > 0) // Mapa 1 não tem inconc, assume que se não tá completo, tá pendente
                            {
                                bool estaIncompleto = LocalizarImagem($"conclusoes/{arquivosInconc[i]}", 0.8) != Point.Empty;
                                if (estaIncompleto)
                                {
                                    Console.WriteLine($"[ALVO ENCONTRADO] Falta completar: {nomesMapas[i]}");
                                    _mapaObrigatorio = nomesMapas[i];
                                    return true;
                                }
                                else
                                {
                                    // Se não está completo E não está "incompleto", então está bloqueado/trancado.
                                    // Para de checar este personagem.
                                    break;
                                }
                            }
                            else
                            {
                                // Lógica para o primeiro mapa (Patio Osseo) se não estiver completo
                                Console.WriteLine($"[ALVO ENCONTRADO] Falta completar: {nomesMapas[i]}");
                                _mapaObrigatorio = nomesMapas[i];
                                return true;
                            }
                        }

                        Console.WriteLine("Este personagem já completou tudo disponivel. Próximo...");
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Erro analise: {ex.Message}"); }

            Console.WriteLine("[AVISO] Nenhum progresso pendente achado. Aleatório.");
            return EscolherPersonagemAleatorio();
        }

        static bool EscolherPersonagemAleatorio()
        {
            try
            {
                string pastaPersonagens = Path.Combine(BaseDir, "Images", "personagens");
                var arquivos = Directory.GetFiles(pastaPersonagens, "*.png");
                Random rng = new Random();
                var arquivosEmbaralhados = arquivos.OrderBy(a => rng.Next()).ToList();
                foreach (var arquivo in arquivosEmbaralhados)
                {
                    string nomeArquivo = Path.GetFileName(arquivo);
                    if (nomeArquivo.ToLower().Contains("radical") || nomeArquivo.ToLower().Contains("header") || nomeArquivo.ToLower().Contains("menu")) continue;
                    Point pos = LocalizarImagem($"personagens/{nomeArquivo}", 0.8);
                    if (pos != Point.Empty) { Clicar(pos.X, pos.Y); return true; }
                }
            }
            catch { }
            return false;
        }

        static string SortearMapaAlvo()
        {
            try
            {
                string pastaMapas = Path.Combine(BaseDir, "Images", "mapas");
                var arquivos = Directory.GetFiles(pastaMapas, "*.png");
                Random rng = new Random();
                var mapasValidos = arquivos.Where(f => {
                    string nome = Path.GetFileName(f).ToLower();
                    return !nome.Contains("novo_jogo") && !nome.Contains("bloqueado") && !nome.Contains("vaziovasto") && !nome.Contains("seta");
                }).ToList();
                if (mapasValidos.Count > 0) return Path.GetFileName(mapasValidos[rng.Next(mapasValidos.Count)]);
            }
            catch { }
            return "";
        }

        static bool TentarEncontrarEClicar(string caminhoRelativo, int timeoutSegundos)
        {
            int tentativas = 0;
            while (tentativas < timeoutSegundos)
            {
                Point pos = LocalizarImagem(caminhoRelativo, 0.7);
                if (pos != Point.Empty) { Clicar(pos.X, pos.Y); return true; }
                Thread.Sleep(1000); tentativas++;
            }
            return false;
        }

        static bool AguardarImagem(string caminhoRelativo, int timeoutSegundos)
        {
            int t = 0;
            while (t < timeoutSegundos)
            {
                if (LocalizarImagem(caminhoRelativo, 0.7) != Point.Empty) return true;
                Console.Write("."); Thread.Sleep(1000); t++;
            }
            return false;
        }

        static Point LocalizarImagem(string caminhoRelativo, double confiancaMinima)
        {
            caminhoRelativo = caminhoRelativo.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string caminhoCompleto = Path.Combine(BaseDir, "Images", caminhoRelativo);

            if (!File.Exists(caminhoCompleto))
            {
                string nome = Path.GetFileName(caminhoCompleto);
                string alt = Path.Combine(BaseDir, "Images", nome);
                if (File.Exists(alt)) caminhoCompleto = alt;
                else return Point.Empty;
            }

            using (Bitmap printScreen = CapturarTela())
            using (Mat matTelaBruta = BitmapConverter.ToMat(printScreen))
            using (Mat matTemplate = Cv2.ImRead(caminhoCompleto, ImreadModes.Color))
            using (Mat matTela3Canais = new Mat())
            using (Mat resultado = new Mat())
            {
                if (matTemplate.Empty()) return Point.Empty;
                if (matTelaBruta.Channels() == 4) Cv2.CvtColor(matTelaBruta, matTela3Canais, ColorConversionCodes.BGRA2BGR);
                else matTelaBruta.CopyTo(matTela3Canais);

                if (matTela3Canais.Width < matTemplate.Width || matTela3Canais.Height < matTemplate.Height) return Point.Empty;

                Cv2.MatchTemplate(matTela3Canais, matTemplate, resultado, TemplateMatchModes.CCoeffNormed);
                double minVal, maxVal;
                OpenCvSharp.Point minLoc, maxLoc;
                Cv2.MinMaxLoc(resultado, out minVal, out maxVal, out minLoc, out maxLoc);

                if (maxVal >= confiancaMinima)
                    return new Point(maxLoc.X + (matTemplate.Width / 2), maxLoc.Y + (matTemplate.Height / 2));
            }
            return Point.Empty;
        }

        static Bitmap CapturarTela()
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap)) { g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size); }
            return bitmap;
        }

        static void Clicar(int x, int y)
        {
            SetCursorPos(x, y); Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0); Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private static void SegurarBotaoDireito(int x, int y, int tempoMs)
        {
            SetCursorPos(x, y); Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0); Thread.Sleep(tempoMs);
            mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }

        static void ScrollMouse(int quantidade, Point alvo)
        {
            SetCursorPos(alvo.X, alvo.Y); Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, quantidade, 0);
        }
    }
}