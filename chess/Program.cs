using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;
using System.Windows.Forms;
using System.Windows;
using System.IO;

namespace chess
{
    class Program : GameWindow
    {
        static void Main()
        {
            Program E;

            try
            {
                E = new Program(800, 800, "Wizlon Chess");
                E.Run(60);
            }
            catch
            {
                MessageBox.Show("ERROR!");
            }
        }

        const int E = 0, P = 1, R = 2, B = 3, H = 4, K = 5, Q = 6, 
            SQRNUM = 64, SQRSIZE = 100, RN = 8, HALFSQRSIZE = SQRSIZE / 2, RNSUBONE = RN - 1;
        int mouseHoverIndex = -1, selectIndex = -1;
        Vector2d[] chessBoardPositions = new Vector2d[SQRNUM];
        bool bw = true, turn = false;
        Color ColorA = Color.White, ColorB = Color.Black, ColorC = Color.Violet;
        List<int> moveList = new List<int>();
        TysonBitmap whitePawn, whiteRook, whiteBishop, whiteKnight, whiteKing, whiteQueen,
            blackPawn, blackRook, blackBishop, blackKnight, blackKing, blackQueen;

        int[] chessBoard =
        {
            +R, +H, +B, +K, +Q, +B, +H, +R,
            +P, +P, +P, +P, +P, +P, +P, +P,
            +E, +E, +E, +E, +E, +E, +E, +E,
            +E, +E, +E, +E, +E, +E, +E, +E,
            -E, -E, -E, -E, -E, -E, -E, -E,
            -E, -E, -E, -E, -E, -E, -E, -E,
            -P, -P, -P, -P, -P, -P, -P, -P,
            -R, -H, -B, -K, -Q, -B, -H, -R,
        };

        Program(int _windowWidth, int _windowHeight, string _windowName) : base(_windowWidth, _windowHeight, GraphicsMode.Default, _windowName)
        {
            // LOAD ASSETS

            /* FIX */
            string assetOriginDir = Path.GetFullPath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"..\..\assets\"));
            string directory = Directory.GetCurrentDirectory() + @"\assets\";
            Directory.CreateDirectory(directory);
            FileInfo[] Files = new DirectoryInfo(assetOriginDir).GetFiles();
            foreach (FileInfo file in Files)
            {
                string dest = directory + file.Name;
                if (!File.Exists(dest)) File.Copy(assetOriginDir + file.Name, dest);
            }

      
            Icon = new Icon(directory + "icon.ico");
            whitePawn = new TysonBitmap(directory + "whitePawn.png");
            whiteRook = new TysonBitmap(directory + "whiteRook.png");
            whiteBishop = new TysonBitmap(directory + "whiteBishop.png");
            whiteKnight = new TysonBitmap(directory + "whiteKnight.png");
            whiteKing = new TysonBitmap(directory + "whiteKing.png");
            whiteQueen = new TysonBitmap(directory + "whiteQueen.png");
            blackPawn = new TysonBitmap(directory + "blackPawn.png");
            blackRook = new TysonBitmap(directory + "blackRook.png");
            blackBishop = new TysonBitmap(directory + "blackBishop.png");
            blackKnight = new TysonBitmap(directory + "blackKnight.png");
            blackKing = new TysonBitmap(directory + "blackKing.png");
            blackQueen = new TysonBitmap(directory + "blackQueen.png");

            // SETUP
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Viewport(ClientRectangle);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 0);
            for (int i = 0; i < SQRNUM; i++) chessBoardPositions[i] = new Vector2d(HALFSQRSIZE + ((i % RN) * SQRSIZE), HALFSQRSIZE + ((i / RN) * SQRSIZE));
            swapTurn();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            mouseHoverIndex = getHover(new Vector2d(Mouse.X, Mouse.Y));
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);

            moveList.Clear();

            // render board
            for (int i = 0; i < SQRNUM; i++)
            {
                Color squareColor = ColorB;
                if (bw) squareColor = ColorA;
                renderSquare(chessBoardPositions[i], squareColor);
                if (i % RN != RNSUBONE) bw = !bw;
            }

            // render pieces
            for (int i = 0; i < SQRNUM; i++)
            {
                Vector2d pos = chessBoardPositions[i];

                switch (chessBoard[i])
                {
                    case P:
                        renderImage(pos, whitePawn);
                        break;
                    case R:
                        renderImage(pos, whiteRook);
                        break;
                    case B:
                        renderImage(pos, whiteBishop);
                        break;
                    case H:
                        renderImage(pos, whiteKnight);
                        break;
                    case K:
                        renderImage(pos, whiteKing);
                        break;
                    case Q:
                        renderImage(pos, whiteQueen);
                        break;
                    case -P:
                        renderImage(pos, blackPawn);
                        break;
                    case -R:
                        renderImage(pos, blackRook);
                        break;
                    case -B:
                        renderImage(pos, blackBishop);
                        break;
                    case -H:
                        renderImage(pos, blackKnight);
                        break;
                    case -K:
                        renderImage(pos, blackKing);
                        break;
                    case -Q:
                        renderImage(pos, blackQueen);
                        break;
                }
            }


            // select
            if (selectIndex != -1)
            {
                int y = getY(selectIndex), x = getX(selectIndex);

                switch (chessBoard[selectIndex])
                {
                    // pawn
                    case P:
                        int q = selectIndex + RN + 1;
                        if (!isSpaceEmpty(q) && x < 7) addMove(q);
                        q = selectIndex + RN - 1;
                        if (!isSpaceEmpty(q) && x > 0) addMove(q);
                        q = selectIndex + RN;
                        if (!isSpaceEmpty(q)) break;
                        addMove(q);
                        if (y == 1) addMove(selectIndex + RN + RN);
                        break;
                    case -P:
                        q = selectIndex - RN + 1;
                        if (!isSpaceEmpty(q) && x < 7) addMove(q);
                        q = selectIndex - RN - 1;
                        if (!isSpaceEmpty(q) && x > 0) addMove(q);
                        q = selectIndex - RN;
                        if (!isSpaceEmpty(q)) break;
                        addMove(q);
                        if (y == 6) addMove(selectIndex - RN - RN);
                        break;

                    // rook
                    case R:
                    case -R:
                        addRookMoves(selectIndex);
                        break;

                    // bishop
                    case B:
                    case -B:
                        addBishopMoves(selectIndex);
                        break;

                    // knight
                    case H:
                    case -H:
                        if (x > 0)
                        {
                            addMove(selectIndex - 17);
                            addMove(selectIndex + 15);
                        }
                        if (x > 1)
                        {
                            addMove(selectIndex - 10);
                            addMove(selectIndex + 6);
                        }
                        if (x < 6)
                        {
                            addMove(selectIndex + 10);
                            addMove(selectIndex - 6);
                        }
                        if (x < 7)
                        {
                            addMove(selectIndex + 17);
                            addMove(selectIndex - 15);
                        }
                        break;

                    // queen
                    case Q:
                    case -Q:
                        addBishopMoves(selectIndex);
                        addRookMoves(selectIndex);
                        break;

                    // king
                    case K:
                    case -K:
                        addMove(selectIndex + RN);
                        addMove(selectIndex - RN);
                        addMove(selectIndex + 1);
                        addMove(selectIndex - 1);
                        addMove(selectIndex + RN + 1);
                        addMove(selectIndex + RN - 1);
                        addMove(selectIndex - RN + 1);
                        addMove(selectIndex - RN - 1);
                        break;
                }

                for (int i = 0; i < moveList.Count; i++) renderSquare(chessBoardPositions[moveList[i]], Color.FromArgb(100, ColorC));

            }

            if (mouseHoverIndex != -1)
            {
                if (turn)
                {
                    if (chessBoard[mouseHoverIndex] > 0) renderSquare(chessBoardPositions[mouseHoverIndex], Color.FromArgb(50, ColorC));
                }
                else
                {
                    if (chessBoard[mouseHoverIndex] < 0) renderSquare(chessBoardPositions[mouseHoverIndex], Color.FromArgb(50, ColorC));
                }
            }

            SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (mouseHoverIndex != -1)
            {
                if (selectIndex != -1)
                {
                    int moveIndex = -1;
                    for (int i = 0; i < moveList.Count; i++) if (mouseHoverIndex == moveList[i]) moveIndex = moveList[i];
                    if (moveIndex != -1)
                    {
                        movePiece(selectIndex, moveIndex);
                        if (getY(moveIndex) == 0 || getY(moveIndex) == 7)
                        {
                            if (chessBoard[moveIndex] == P) chessBoard[moveIndex] = Q;
                            else if (chessBoard[moveIndex] == -P) chessBoard[moveIndex] = -Q;
                        }
                        mouseHoverIndex = -1;
                        selectIndex = -1;
                        moveIndex = -1;
                        swapTurn();
                    }
                }
            }

            if (mouseHoverIndex != -1)
            {
                if (chessBoard[mouseHoverIndex] != E)
                {
                    if (mouseHoverIndex == selectIndex) selectIndex = -1;
                    else
                    {
                        if (turn)
                        {
                            if (chessBoard[mouseHoverIndex] > 0) selectIndex = mouseHoverIndex;
                        }
                        else
                        {
                            if (chessBoard[mouseHoverIndex] < 0) selectIndex = mouseHoverIndex;
                        }
                    }
                }
                else selectIndex = -1;
            }
            base.OnMouseDown(e);
        }

        void renderImage(Vector2d _pos, TysonBitmap _pic)
        {
            Toolbox.rIMAGE(_pic, _pos.X, _pos.Y, 25, 52);
        }

        void movePiece(int _indexA, int _indexB)
        {
            chessBoard[_indexB] = chessBoard[_indexA];
            chessBoard[_indexA] = E;
        }

        int getX(int _index)
        {
            return _index % RN;
        }

        int getY(int _index)
        {
            return _index / RN;
        }

        bool isSpaceEmpty(int _index)
        {
            bool export = false;
            if (isValidIndex(_index))
            {
                if (chessBoard[_index] == E) export = true;
            }
            return export;
        }

        void addBishopMoves(int _si)
        {
            for (int i = 0; i < RN - (_si % RN) - 1; i++)
            {
                int q = (_si + (RN * (i + 1))) + (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < _si % RN; i++)
            {
                int q = (_si + (RN * (i + 1))) - (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < RN - (_si % RN) - 1; i++)
            {
                int q = (_si - (RN * (i + 1))) + (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < _si % RN; i++)
            {
                int q = (_si - (RN * (i + 1))) - (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
        }

        void addRookMoves(int _si)
        {
            for (int i = 0; i < RN; i++)
            {
                int q = _si + (RN * (i + 1));
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < RN; i++)
            {
                int q = _si - (RN * (i + 1));
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < RN - (_si % RN) - 1; i++)
            {
                int q = _si + (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
            for (int i = 0; i < _si % RN; i++)
            {
                int q = _si - (i + 1);
                addMove(q);
                if (!isSpaceEmpty(q)) break;
            }
        }

        void addMove(int moveIndex)
        {
            if (isValidIndex(moveIndex))
            {
                if (turn)
                {
                    if (chessBoard[moveIndex] <= 0) moveList.Add(moveIndex);
                }
                else
                {
                    if (chessBoard[moveIndex] >= 0) moveList.Add(moveIndex);
                }
            }
        }

        void swapTurn()
        {
            turn = !turn;
            if (turn) ColorC = Color.Red;
            else ColorC = Color.Blue;
        }

        bool isValidIndex(int _index)
        {
            return (_index > -1 && _index < SQRNUM);
        }

        int getHover(Vector2d m)
        {
            int export = -1;

            for (int i = 0; i < SQRNUM; i++)
            {
                Vector2d c = chessBoardPositions[i];
                if (m.X < c.X + HALFSQRSIZE &&
                    m.X > c.X - HALFSQRSIZE &&
                    m.Y < c.Y + HALFSQRSIZE &&
                    m.Y > c.Y - HALFSQRSIZE) export = i;
            }

            return export;
        }

        void renderSquare(Vector2d _pos, Color _hue)
        {
            Toolbox.rBOX(new Vector2d(_pos.X - HALFSQRSIZE, _pos.Y - HALFSQRSIZE), SQRSIZE, _hue);
        }
    }
}
