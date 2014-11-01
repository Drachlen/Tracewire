using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.IO;

namespace Tracewire
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        public KeyboardState CurrentKeyboardState;
        public KeyboardState LastKeyboardState;

        public MouseState CurrentMouseState;
        public MouseState LastMouseState;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D T_BG;
        Texture2D T_Bomb;
        Texture2D T_WhiteDot;
        Texture2D T_Wire;
        Texture2D T_WireShadow;
        Texture2D T_Node;
        Texture2D T_NodeSpecial;
        Texture2D T_TimerBG;
        Texture2D T_Arrow;

        SpriteFont F_Timer;

        SoundEffect S_Fail;
        SoundEffect S_Good;
        SoundEffect S_FinishLevel;
        SoundEffect S_FinishWire;


        //Song S_Music;

        //Song S_Music;

        SoundEffect S_MusicWav;

        Rectangle GameArea;

        Node[,] Grid;
        int GridWidth = 15;
        int GridHeight = 8;

        Point SelectedNode;
        Point StartPosition;


        int SelectedWire = -1;

        double FlashTimer;

        bool Flash;

        bool PauseTimer = true;


        int StartingWire = -1;

        double Timer;

        int CurrentLevel;

        int[] WireStrength;

        double TotalPlayTime = 0;
        public static Random GRand;

        public Game1()
        {
            this.Window.Title = "Tracewire";
            GRand = new Random();

            WireStrength = new int[10];

            ResetWireStrength();

            CurrentLevel = 1;

            CurrentKeyboardState = new KeyboardState();
            LastKeyboardState = new KeyboardState();

            FlashTimer = 0;
            Flash = false;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 896;
            graphics.PreferredBackBufferHeight = 504;

            GameArea = new Rectangle(73, 52, 750, 400);

            ResetEverything();

            //LoadLevel(1);

            LoadLevelFromFile(CurrentLevel);

            
        }

        public void ResetWireStrength()
        {
            for (int i = 0; i < 10; i++)
            {
                WireStrength[i] = 12;
            }
        }

        public void SelectWire(int ID)
        {
            SelectedWire = ID;
            StartPosition = NextWireStart(SelectedWire);
            SelectedNode = StartPosition;
            ResetTimer();
            
        }

        public Point NextWireStart(int WireID)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Node Next = Grid[0, y];
                if (Next.Color == WireID)
                {
                    return new Point(0, y);
                }
            }
            return new Point(-1, -1);
        }

        Point LastNode = new Point(-1, -1);
        int LastColor = -1;
        public void LevelAddNode(int Color, Point pos, bool special=false)
        {
            if (LastColor != Color)
            {
                LastNode = new Point(-1, -1);
                LastColor = Color;
            }
            Grid[pos.X, pos.Y] = new Node(Color, LastNode, special);
            if(LastNode.X != -1)
                Grid[LastNode.X, LastNode.Y].Connection = pos;
            if (pos.X == 14)
            {
                Debug.WriteLine("END WIRE");
                Grid[pos.X, pos.Y].Connection = pos;
            }
            LastNode = pos;
        }

        public void ResetTimer()
        {
            PauseTimer = true;
            Timer = 10000;
        }

        public void ResetEverything()
        {
            
            ResetTimer();
            Grid = new Node[GridWidth, GridHeight];

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Grid[x, y] = new Node(-1, new Point(-1, -1), false);
                }
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            T_BG = Content.Load<Texture2D>("bg");
            T_TimerBG = Content.Load<Texture2D>("timerbg");
            T_Bomb = Content.Load<Texture2D>("bomb");
            T_WhiteDot = Content.Load<Texture2D>("dot");
            T_Wire = Content.Load<Texture2D>("wire");
            T_WireShadow = Content.Load<Texture2D>("wireshadow");
            T_Node = Content.Load<Texture2D>("node");
            T_NodeSpecial = Content.Load<Texture2D>("rednode");
            T_Arrow = Content.Load<Texture2D>("arrow");
            F_Timer = Content.Load<SpriteFont>("Timer");
            S_Fail = Content.Load<SoundEffect>("fail");
            S_Good = Content.Load<SoundEffect>("good");
            S_FinishLevel = Content.Load<SoundEffect>("finishlevel");            
            S_FinishWire = Content.Load<SoundEffect>("finishwire");
            S_MusicWav = Content.Load<SoundEffect>("Backbeatwav");
            //S_Music = Content.Load<Song>("Backbeat");

            //MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(S_Music);

            /*
            SoundEffectInstance MusicInstance = S_Music.CreateInstance();
            MusicInstance.IsLooped = true;
            S_Music.Play(0.1f, 0f, 0f);
            */
            SoundEffectInstance MusicWaveInstance = S_MusicWav.CreateInstance();
            MusicWaveInstance.IsLooped = true;
            MusicWaveInstance.Play();
            //Music.Play(0.3f, 0f, 0f);
        }

        protected override void UnloadContent()
        {
        }
        
        public bool NewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key));
        }

        public void UnfadeWire(int ID)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    if (Next.Color == ID)
                    {
                        Next.Faded = false;
                        Next.Minimum = 0;
                        Next.SpecialTypePassed = false;
                    }
                }
            }
            WireStrength[ID] = 12;
        }

        public void UpdateMinimum()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    if (Next.Faded && Next.Minimum < 11)
                    {
                        Next.Minimum += 1;
                        
                    }
                }
            }
        }

        bool Exploding = false;

        public void UpdateExplode()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];

                    if(Next.Exploding == false)
                    {
                        Next.Velocity = 50;
                        Next.Exploding = true;
                    }

                    Next.Position.X -= (float)(Math.Cos(-Next.Angle) * -(Next.Velocity));
                    Next.Position.Y -= (float)(Math.Sin(-Next.Angle) * (Next.Velocity));
                }
            }
        }

        public void UpdateGame(GameTime gameTime)
        {

            if (Exploding)
            {
                UpdateExplode();
                return;
            }

            if (!PauseTimer)
            {
                Timer -= gameTime.ElapsedGameTime.Milliseconds;
                TotalPlayTime += gameTime.ElapsedGameTime.Milliseconds;
            }

            FlashTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (EndOfLevels)
            {
                PauseTimer = true;
                if (NewKeyPress(Keys.Space))
                {
                    EndOfLevels = false;
                    TotalPlayTime = 0;
                    CurrentLevel = 1;
                    LoadLevelFromFile(CurrentLevel);
                }
                return;
            }

            if (Timer <= 0)
            {
                PauseTimer = true;

                if (NewKeyPress(Keys.Space))
                {
                    TotalPlayTime = 0;
                    CurrentLevel = 1;
                    LoadLevelFromFile(CurrentLevel);
                }
                //Exploding = true;
                return;
            }

            UpdateMinimum();

            if (SelectedNode.X == -1)
            {
                S_FinishLevel.Play(0.1f, 0f, 0f);
                Debug.WriteLine("level complete!");
                CurrentLevel++;
                LoadLevelFromFile(CurrentLevel);
                return;
            }
            Node CurrentNode = Grid[SelectedNode.X, SelectedNode.Y];

            Node NextNode = null;
            int ValidDirection = -1;
            bool AcceptMove = false;
            bool BadMove = false;
            if (CurrentNode.Connection.X != SelectedNode.X || CurrentNode.Connection.Y != SelectedNode.Y)
            {
                //Debug.WriteLine("ok");
                NextNode = Grid[CurrentNode.Connection.X, CurrentNode.Connection.Y];



                if (SelectedNode.X < CurrentNode.Connection.X)
                    ValidDirection = 2;

                if (SelectedNode.X > CurrentNode.Connection.X)
                    ValidDirection = 4;

                if (SelectedNode.Y < CurrentNode.Connection.Y)
                    ValidDirection = 3;

                if (SelectedNode.Y > CurrentNode.Connection.Y)
                    ValidDirection = 1;

                if (CurrentNode.SpecialType && !CurrentNode.SpecialTypePassed)
                    ValidDirection = 0;
            }
            else
            {
                Debug.WriteLine("jumping wires! Current wire: "+SelectedWire);
                SelectWire(SelectedWire + 1);
                PauseTimer = false;
                S_FinishWire.Play(0.1f, 0f, 0f);
                
            }

            if (NewKeyPress(Keys.D))
            {
                Debug.WriteLine(ValidDirection + ", " + SelectedNode + ", " + CurrentNode.Connection);
            }

            if (NewKeyPress(Keys.Space))
            {
                if (ValidDirection == 0)
                    AcceptMove = true;
                else
                    BadMove = true;
            }

            if (NewKeyPress(Keys.Left))
            {
                if (ValidDirection == 4)
                    AcceptMove = true;
                else
                    BadMove = true;
            }

            if (NewKeyPress(Keys.Right))
            {
                if (ValidDirection == 2)
                    AcceptMove = true;
                else
                    BadMove = true;
            }

            if (NewKeyPress(Keys.Up))
            {
                if (ValidDirection == 1)
                    AcceptMove = true;
                else
                    BadMove = true;
            }

            if (NewKeyPress(Keys.Down))
            {
                if (ValidDirection == 3)
                    AcceptMove = true;
                else
                    BadMove = true;
            }

            if (BadMove)
            {
                S_Fail.Play(0.1f, 0f, 0f);
                PauseTimer = false;
                SelectedNode = StartPosition;
                UnfadeWire(SelectedWire);
            }
            else
                if (AcceptMove)
                {
                    S_Good.Play(0.1f, 0f, 0f);
                    PauseTimer = false;
                    if (CurrentNode.SpecialType && !CurrentNode.SpecialTypePassed)
                    {
                        CurrentNode.SpecialTypePassed = true;
                    }
                    else
                    {
                        //WireStrength[CurrentNode.Color] -= 1;
                        Debug.WriteLine("Connect to: "+CurrentNode.Connection);
                        CurrentNode.Faded = true;
                        SelectedNode = CurrentNode.Connection;
                        Debug.WriteLine("New Selection is: " + SelectedNode);
                    }
                }

            if (FlashTimer > 50)
            {
                FlashTimer = 0;
                Flash = (Flash) ? false : true;
            }
        }

        public bool KeyState(Keys k)
        {
            return CurrentKeyboardState.IsKeyDown(k);
        }

        Rectangle MouseRect()
        {
            return new Rectangle(MousePosition.X, MousePosition.Y, 24, 24);
        }

        Point MousePosition;
        Point HoverNode;


        public void ExportLevel()
        {
            Debug.WriteLine("export");
            using (StreamWriter writer = new StreamWriter("Content/Levels.txt", true))
            {
                writer.WriteLine(MapData);
            }
        }


        bool EndOfLevels = false;

        public void LoadLevelFromFile(int ID)
        {
            ResetEverything();
            ResetWireStrength();

            

            ID -= 1;
            string LevelData = "";
            int CurrentLine = 0;
            string LastData = "";
            using (StreamReader reader = new StreamReader("Content/Levels.txt"))
            {
                string tmp;
                while ((tmp = reader.ReadLine()) != null)
                {
                    LastData = tmp;
                    if (CurrentLine == ID)
                    {
                        LevelData = tmp;
                        break;
                    }
                    CurrentLine++;
                }

                
            }

            if (ID > 9000)
            {
                LevelData = LastData;
            }
            Debug.WriteLine(LevelData);

            if (LevelData.Length < 1)
            {
                Debug.WriteLine("NO LEVEL DATA");
                EndOfLevels = true;
                return;
            }

            string[] data = LevelData.Split(',');


            Debug.WriteLine(CurrentLine);


            int Step = 0;
            int X = -1;
            int Y = -1;
            int Color = -1;
            bool Special = false;
            foreach (string val in data)
            {
                int RealValue;
                int.TryParse(val, out RealValue);
                //Debug.WriteLine("Step: " + Step);
                switch (Step)
                {
                    case 0:
                        //Debug.WriteLine("X");
                        X = RealValue;
                        break;
                    case 1:
                        //Debug.WriteLine("Y");
                        Y = RealValue;
                        break;
                    case 2:
                        //Debug.WriteLine("Color");
                        Color = RealValue;
                        break;
                    case 3:
                        //Debug.WriteLine("Special");
                        if (RealValue == 1)
                            Special = true;
                        break;
                }


                if (Step == 3)
                {
                    //Debug.WriteLine("adding node: Color: " + Color + ", X: " + X + ", Y: " + Y + "");
                    LevelAddNode(Color, new Point(X, Y), Special);

                    X = -1;
                    Y = -1;
                    Color = -1;
                    Special = false;
                    Step = 0;
                }
                else
                {
                    Step++;
                }
            }

            SelectWire(1);
        }


        public void UpdateEditor(GameTime gameTime)
        {

            if (KeyState(Keys.LeftControl))
            {

                if (KeyState(Keys.X))
                {
                    ResetEverything();
                    ResetEditor();
                }

                if (NewKeyPress(Keys.E))
                {
                    ExportLevel();
                }
            }



            HoverNode = new Point(-1, -1);
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];

                    Rectangle NodeDest = new Rectangle(GameArea.X + 10 + (50 * x), GameArea.Y + 10 + (50 * y), 28, 28);

                    if( MouseRect().Intersects(NodeDest))
                    {
                        HoverNode = new Point(x, y);
                    }
                }
            }

            if (NewLeftMouseClick())
            {
                if (HoverNode.X != -1)
                {
                    Node Hovered = Grid[HoverNode.X, HoverNode.Y];

                    Debug.WriteLine("col: "+Hovered.Color);

                    if (Hovered.Color == -1)
                    {
                        if ( (HoverNode.X != LastNode.X || HoverNode.Y != LastNode.Y) && MapData.Length > 1)
                            MapData += ",";
                        LevelAddNode(SelectedWire, new Point(HoverNode.X, HoverNode.Y));

                        MapData += HoverNode.X + "," + HoverNode.Y + "," + SelectedWire + ",0";
                        
                        if (HoverNode.X == 14)
                        {
                            SelectedWire++;
                        }
                    }
                    else if (HoverNode.X == LastNode.X && HoverNode.Y == LastNode.Y)
                    {
                        if (Hovered.SpecialType)
                        {
                            Hovered.SpecialType = false;
                            string Adjustment = MapData.Substring(0, MapData.Length - 1);
                            MapData = Adjustment + "0";
                        }
                        else
                        {
                            Hovered.SpecialType = true;
                            string Adjustment = MapData.Substring(0, MapData.Length - 1);
                            MapData = Adjustment + "1";
                        }
                    }

                    //Debug.WriteLine(MapData);
                }
            }

        }

        string MapData;
        public void ResetEditor()
        {
            MapData = "";
            LastNode = new Point(-1, -1);
            LastColor = -1;
            SelectedWire = 1;
        }

        public bool NewLeftMouseClick()
        {
            return (CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released);
        }


        public bool EditMode = false;

        protected override void Update(GameTime gameTime)
        {

            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            MousePosition = new Point(CurrentMouseState.X, CurrentMouseState.Y);


            if (NewKeyPress(Keys.OemTilde))
            {
                if (EditMode)
                {
                    CurrentLevel = 1;
                    LoadLevelFromFile(9999);
                    EditMode = false;
                }
                else
                {
                    
                    ResetEverything();
                    ResetEditor();
                    EditMode = true;
                }
            }

            if (EditMode)
            {
                UpdateEditor(gameTime);
            }
            else
            {
                UpdateGame(gameTime);
            }

            base.Update(gameTime);
        }

        /*
         * Game area:
         * 750x400
         * 
         * Wire graphic: 20x12
         * Wire shadow: 38x28
         * 
         **/

        public void DrawWin()
        {
            string DisplayTotal = (TotalPlayTime / 1000).ToString("0.00");
            spriteBatch.DrawString(F_Timer, "You traced all the wires", new Vector2(50, 50), Color.Green);
            spriteBatch.DrawString(F_Timer, "You defused " + (CurrentLevel - 1) + " bombs.\nIt only took you " + DisplayTotal + " Seconds.", new Vector2(50, 250), Color.White);
            spriteBatch.DrawString(F_Timer, "Press Space to play again.", new Vector2(50, 360), new Color(0, 150, 45));

            //spriteBatch.DrawString(F_Timer, CurrentLevel.ToString(), new Vector2(10, 0), new Color(30, 30, 30, 30));
        }

        public void DrawLose()
        {
            string DisplayTotal = (TotalPlayTime/1000).ToString("0.00");
            spriteBatch.DrawString(F_Timer, "You failed to trace the wires.\nPress Space to try again.", new Vector2(50, 50), Color.Red);
            spriteBatch.DrawString(F_Timer, "You defused "+(CurrentLevel - 1)+" bombs.\nYou traced for "+DisplayTotal+" Seconds.", new Vector2(50, 350), Color.White);

            //spriteBatch.DrawString(F_Timer, CurrentLevel.ToString(), new Vector2(10, 0), new Color(30, 30, 30, 30));
        }

        public void DrawGame()
        {
            if (EndOfLevels)
            {
                DrawWin();
                return;
            }
            if (Timer <= 0)
            {
                DrawLose();
                return;
            }
            DrawBackground();
            if(!Exploding)
                DrawWireShadows();
            DrawWires();
            DrawNodes();
        }

        public void DrawEditor()
        {
            DrawBackground();
            DrawNodePositions();
            DrawWires();
            DrawNodes();
            DrawMouse();
        }

        public void DrawMouse()
        {
            Rectangle Dest = new Rectangle(MousePosition.X, MousePosition.Y, 24, 24);
            spriteBatch.Draw(T_Arrow, Dest, Color.White);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (EditMode)
            {
                DrawEditor();
            }
            else
            {
                DrawGame();
            }
           
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        

        public void DrawBackground()
        {

            Color BombColor = Color.White;


            int Adjustment = (int)Math.Round(Timer / 100)*2;

            int av = 200 - Adjustment;

            BombColor = new Color(255, 255 - av, 255 - av);

            Color BGC = new Color(0 + av, 0 + (av/4), 0 + (av/4));

            Rectangle Dest = new Rectangle(0, 0, 896, 504);
            spriteBatch.Draw(T_BG, Dest, BGC);

            Rectangle BombDest = new Rectangle(23, 27, 850, 450);
            spriteBatch.Draw(T_Bomb, BombDest, BombColor);

            Rectangle TimerBGDest = new Rectangle(693, 63, 135, 69);
            spriteBatch.Draw(T_TimerBG, TimerBGDest, Color.White);

            double TimerD = Math.Round((Timer / 1000), 2);

            string Display = TimerD.ToString("0.00");

            int ExtraX = 0;

           // Debug.WriteLine(Display);

            spriteBatch.DrawString(F_Timer, Display, new Vector2(705 + ExtraX, 70), Color.Red);

            spriteBatch.DrawString(F_Timer, CurrentLevel.ToString(), new Vector2(10, 0), new Color(70, 70, 70, 70));
        }

        public virtual void DrawWires()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    if (Next.Color == -1)
                        continue;
                    Color DrawColor = new Color(200, 200, 200, 150);

                    switch (Next.Color)
                    {
                        case 1:
                            DrawColor = new Color(251, 69, 21);
                            break;
                        case 2:
                            DrawColor = new Color(105, 167, 237);
                            break;
                        case 3:
                            DrawColor = new Color(95, 202, 81);
                            break;
                        case 4:
                            DrawColor = new Color(227, 242, 57);
                            break;
                        case 5:
                            DrawColor = new Color(242, 57, 231);
                            break;
                        default:
                            DrawColor = Color.White;
                            break;
                    }

                    if (Next.Faded)
                    {
                        DrawColor = new Color(50, 50, 50, 50);
                        //DrawColor.R -= (byte)Next.Colorize;
                        //DrawColor.G -= (byte)Next.Colorize;
                        //DrawColor.B -= (byte)Next.Colorize;
                    } else
                    if (Flash == true && SelectedNode.X == x && SelectedNode.Y == y)
                    {
                        if (!Next.SpecialType || Next.SpecialTypePassed)
                            DrawColor.A = 50;
                    }

                    

                    Point A = new Point(x, y);
                    Point B = Next.Connection;

                    Point Start = new Point(-1, -1);
                    Point End = new Point(-1, -1);
                    if (A.X < B.X)
                    {
                        Start.X = A.X;
                        End.X = B.X;
                    }
                    else
                    {
                        Start.X = B.X;
                        End.X = A.X;
                    }

                    if (A.Y < B.Y)
                    {
                        Start.Y = A.Y;
                        End.Y = B.Y;
                    }
                    else
                    {
                        Start.Y = B.Y;
                        End.Y = A.Y;
                    }

                    int DX = (End.X - Start.X) * 50;
                    int DY = (End.Y - Start.Y) * 50;

                    Rectangle Source = new Rectangle(0, 0, 20, 12);

                    int BX = GameArea.X + 15;
                    int BY = GameArea.Y + 17;
                    int XOffset = (50 * Start.X);
                    int YOffset = (50 * Start.Y);
                    
                    int Skip = 0;
                    if (Start.X < End.X )
                    {
                        
                        while (XOffset < (End.X * 50) - 20)
                        {
                            Skip++;
                            XOffset += 20;
                            int Shrink = Next.Minimum;

                            Rectangle TmpSource = new Rectangle(0, 0, 20 - Shrink, 12);
                            Rectangle NodeDest = new Rectangle(BX + XOffset + (Shrink / 2) + (int)Next.Position.X, BY + YOffset + (Shrink / 4), 20 - (Shrink / 2) + (int)Next.Position.Y, (WireStrength[Next.Color]) - (Shrink / 2));
                            spriteBatch.Draw(T_Wire, NodeDest, TmpSource, DrawColor);
                        }
                    } else 
                    if (Start.Y < End.Y)
                    {
                        while (YOffset < (End.Y * 50) - 20)
                        {
                            YOffset += 20;
                            Rectangle TmpSource = new Rectangle(0, 0, 20 - Next.Minimum, 12);
                            //Rectangle NodeDest = new Rectangle(BX + XOffset, BY + YOffset, 20, 12);
                            Rectangle NodeDest = new Rectangle(BX + XOffset + (Next.Minimum / 2), BY + 8 + YOffset + (Next.Minimum / 4), 20 - (Next.Minimum / 2), (WireStrength[Next.Color]) - (Next.Minimum / 2));
                            spriteBatch.Draw(T_Wire, NodeDest, TmpSource, DrawColor, (float)(Math.PI/2), new Vector2(12, 15), SpriteEffects.None, 0);
                        }
                    }

                    /*
                    Rectangle NodeDest = new Rectangle(GameArea.X + 16 + 1 + (50 * Start.X), GameArea.Y + 6 + 10 + (50 * Start.Y), DX + 12, DY + 12);
                    

                    Texture2D WWire;

                    if (DX > DY)
                        WWire = T_Wire;
                    else
                        WWire = T_WireVertical;
                    spriteBatch.Draw(WWire, NodeDest, DrawColor);
                    */
                }
            }
        }

        public virtual void DrawNodes()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    if (Next.Color == -1)
                        continue;
                    Color DrawColor = new Color(175, 175, 175, 255);



                    if (SelectedNode.X == x && SelectedNode.Y == y)
                        DrawColor = Color.White;


                    Texture2D NodeTex = T_Node;
                    
                    if (Next.SpecialType && !Next.SpecialTypePassed)
                    {
                        NodeTex = T_NodeSpecial;
                        if (SelectedNode.X == x && SelectedNode.Y == y && Flash == true)
                        {
                            DrawColor.A = 50;
                        }
                    }


                    Rectangle NodeDest = new Rectangle(GameArea.X + 10 + (50 * x) + (int)Next.Position.X, GameArea.Y + 10 + (50 * y) + (int)Next.Position.Y, 28, 28);

                    

                    spriteBatch.Draw(NodeTex, NodeDest, DrawColor);

                    if (SelectedNode.X == x && SelectedNode.Y == y && (!Next.SpecialType || Next.SpecialTypePassed))
                    {
                        float ArrowRotation = -1;

                        if (Next.Connection.X > x)
                            ArrowRotation = (float)Math.PI;
                        if (Next.Connection.X < x)
                            ArrowRotation = 0;
                        if(Next.Connection.Y > y)
                            ArrowRotation = (float)Math.PI*1.5f;
                        if (Next.Connection.Y < y)
                            ArrowRotation = (float)Math.PI/2;

                        Rectangle ArrowSource = new Rectangle(0, 0, 24, 24);
                        Rectangle ArrowDest = new Rectangle(GameArea.X + 10 + 10 +(50 * x), GameArea.Y + 10 + (50 * y), 28, 28);
                        if(ArrowRotation != -1)
                            spriteBatch.Draw(T_Arrow, ArrowDest, ArrowSource, Color.White, ArrowRotation, new Vector2(12, 12), SpriteEffects.None, 0);
                    }

                }
            }
        }



        public virtual void DrawWireShadows()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    if (Next.Color == -1 || Next.Faded)
                        continue;
                    Color DrawColor = new Color(150, 150, 150, 150);

                    

                    Point A = new Point(x, y);
                    Point B = Next.Connection;

                    Point Start = new Point(-1, -1);
                    Point End = new Point(-1, -1);
                    if (A.X < B.X)
                    {
                        Start.X = A.X;
                        End.X = B.X;
                    }
                    else
                    {
                        Start.X = B.X;
                        End.X = A.X;
                    }

                    if (A.Y < B.Y)
                    {
                        Start.Y = A.Y;
                        End.Y = B.Y;
                    }
                    else
                    {
                        Start.Y = B.Y;
                        End.Y = A.Y;
                    }

                    int DX = (End.X - Start.X) * 50;
                    int DY = (End.Y - Start.Y) * 50;

                    Rectangle Source = new Rectangle(0, 0, 20, 12);

                    int BX = GameArea.X + 15;
                    int BY = GameArea.Y + 17;
                    int XOffset = (50 * Start.X);
                    int YOffset = (50 * Start.Y);

                    if (Start.X < End.X)
                    {
                        while (XOffset < (End.X * 50) - 20)
                        {
                            XOffset += 20;
                            Rectangle TmpSource = new Rectangle(0, 0, 38 - Next.Minimum, 28);
                            Rectangle NodeDest = new Rectangle(BX + XOffset + (Next.Minimum / 2), BY + YOffset + (Next.Minimum / 4), 38 - (Next.Minimum / 2), 28 - (Next.Minimum / 2));
                            spriteBatch.Draw(T_WireShadow, NodeDest, TmpSource, DrawColor);
                        }
                    }
                    else if (Start.Y < End.Y)
                    {
                        while (YOffset < (End.Y * 50) - 20)
                        {
                            YOffset += 20;
                            Rectangle TmpSource = new Rectangle(0, 0, 38 - Next.Minimum, 28);
                            Rectangle NodeDest = new Rectangle(BX + XOffset + (Next.Minimum / 2), BY + YOffset + (Next.Minimum / 4), 38 - (Next.Minimum / 2), 28 - (Next.Minimum / 2));
                            spriteBatch.Draw(T_WireShadow, NodeDest, TmpSource, DrawColor, (float)(Math.PI / 2), new Vector2(12, 15 + 10), SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }

        public void DrawGameArea()
        {
            spriteBatch.Draw(T_WhiteDot, GameArea, Color.White);
        }


        //Editor functions
        public virtual void DrawNodePositions()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Node Next = Grid[x, y];
                    Color DrawColor = new Color(175, 175, 175, 50);


                    Texture2D NodeTex = T_Node;

                    if (Next.SpecialType)
                    {
                        NodeTex = T_NodeSpecial;
                    }

                    if (HoverNode.X == x && HoverNode.Y == y)
                    {
                        DrawColor.A += 100;
                    }


                    Rectangle NodeDest = new Rectangle(GameArea.X + 10 + (50 * x), GameArea.Y + 10 + (50 * y), 28, 28);

                    spriteBatch.Draw(NodeTex, NodeDest, DrawColor);
                }
            }
        }


    }
}
