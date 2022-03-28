using JCLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace IndieGameStation
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D buffer;

        public enum ListStyle
        {
            Xbox,
            Wii
        }

        public ListStyle listStyle;

        public static ControlsManager ctrl;

        public static GameItem[] games;
        public static int selectedGameIndex;
        public int scrollBuffer;
        public int scrollX;

        Process process;
        Process antiMicro;

        public string gameDirectory;
        public string[] gameExtensions;
        public string[] imageExtensions;
        public string[] excludedGames;
        public string[] fullscreenExemptions;
        public GameItem selectedGame;
        public string errorMessage;

        public static SpriteFont font;
        Texture2D background;
        Color clearColor;

        float alpha;
        bool isGameRunning;
        int fullscreenCount;
        int exitCount;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.IsBorderless = true;
        }

        protected override void Initialize()
        {
            buffer = new RenderTarget2D(GraphicsDevice, 1600, 960);

            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.ApplyChanges();

            listStyle = ListStyle.Xbox;

            ctrl = new ControlsManager(PlayerIndex.One);
            ctrl.SetupDefaultKeys();

            alpha = 0;
            scrollBuffer = 0;
            scrollX = 0;

            if (File.Exists(@"./extensions.txt"))
                gameExtensions = File.ReadAllLines(@"./extensions.txt");
            else
                errorMessage = "extensions.txt not found!";

            if (File.Exists(@"./directory.txt"))
                gameDirectory = File.ReadAllText(@"./directory.txt");
            else
                errorMessage = "directory.txt not found!";

            if (File.Exists(@"./exclude.txt"))
                excludedGames = File.ReadAllLines(@"./exclude.txt");
            else
                excludedGames = new string[0];

            if (File.Exists(@"./fullscreen_exempt.txt"))
                fullscreenExemptions = File.ReadAllLines(@"./fullscreen_exempt.txt");
            else
                fullscreenExemptions = new string[0];

            imageExtensions = new string[] { "jpg", "JPG", "png", "PNG", "bmp", "BMP", "gif", "GIF" };

            if (string.IsNullOrWhiteSpace(errorMessage) && Directory.Exists(gameDirectory))
            {
                LoadGameList();
                if (games.Length == 0)
                    errorMessage = "No games found in directory!";
            }
            else if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = "Directory not found! Please check directory.txt";

            try
            {
                string[] settings = File.ReadAllText(@"./settings.d").Split(',');
                Settings.Fullscreenizer = bool.Parse(settings[0]);
                Settings.HideMouse = bool.Parse(settings[1]);
                Settings.ExitCombo = int.Parse(settings[2]);
                Settings.ListStyle = int.Parse(settings[3]);
            }
            catch
            {
                Settings.Fullscreenizer = true;
                Settings.HideMouse = true;
                Settings.ExitCombo = 0;
                Settings.ListStyle = 0;
            }
            Settings.Show = false;
            Settings.Selection = 0;
            listStyle = (ListStyle)Settings.ListStyle;

            fullscreenCount = 100;
            exitCount = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("defaultFont");
            clearColor = new Color(255, 255, 255);

            try
            {
                background = LoadImageFromFile("bg.png");
            }
            catch
            {
            }

            Audio.AddSound("positive", Content.Load<SoundEffect>("sfx_positive"));
            Audio.AddSound("toggle", Content.Load<SoundEffect>("sfx_toggle"));
            Audio.AddSound("select", Content.Load<SoundEffect>("sfx_select"));
            Audio.AddSound("switch", Content.Load<SoundEffect>("sfx_switch"));

            Audio.PlaySound("positive");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (isGameRunning)
            {
                if (Settings.Fullscreenizer && !selectedGame.FullscreenExempt)
                {
                    fullscreenCount++;
                    if (fullscreenCount > 200)
                    {
                        if (!WindowHelper.IsFullscreen(process, GraphicsDevice))
                            WindowHelper.Fullscreenize(process, GraphicsDevice);
                        fullscreenCount = 0;
                    }
                }

                if (Settings.HideMouse)
                    Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

                switch (Settings.ExitCombo)
                {
                    case 0:
                        if (GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed &&
                            GamePad.GetState(PlayerIndex.One).Buttons.LeftStick == ButtonState.Pressed)
                            process.Kill();
                        break;
                    case 1:
                        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed &&
                            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                            process.Kill();
                        break;
                    case 2:
                        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                            exitCount++;
                        else
                            exitCount = 0;

                        if (exitCount > 150)
                        {
                            exitCount = 0;
                            process.Kill();
                        }
                        break;
                    case 3:
                        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                            exitCount++;
                        else
                            exitCount = 0;

                        if (exitCount > 150)
                        {
                            exitCount = 0;
                            process.Kill();
                        }
                        break;
                }
                

                if (process.HasExited)
                {
                    if (selectedGame.IncludeKeyboard && !antiMicro.HasExited)
                        antiMicro.Kill();

                    isGameRunning = false;
                    selectedGame = null;
                    Audio.PlaySound("positive");
                }
            }
            else
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    exitCount++;
                else
                    exitCount = 0;

                if (exitCount > 300)
                    Exit();

                if (Settings.Show)
                {
                    if (ctrl.Pressed(Button.Down))
                        Settings.Selection++;
                    else if (ctrl.Pressed(Button.Up))
                        Settings.Selection--;

                    if (Settings.Selection > 4)
                        Settings.Selection = 0;
                    else if (Settings.Selection < 0)
                        Settings.Selection = 4;

                    if (ctrl.Pressed(Button.Left))
                    {
                        switch (Settings.Selection)
                        {
                            case 0: Settings.Fullscreenizer = !Settings.Fullscreenizer; break;
                            case 1: Settings.HideMouse = !Settings.HideMouse; break;
                            case 2: Settings.ExitCombo--; break;
                            case 3: Settings.ListStyle--; break;
                        }
                    }
                    else if (ctrl.Pressed(Button.Right))
                    {
                        switch (Settings.Selection)
                        {
                            case 0: Settings.Fullscreenizer = !Settings.Fullscreenizer; break;
                            case 1: Settings.HideMouse = !Settings.HideMouse; break;
                            case 2: Settings.ExitCombo++; break;
                            case 3: Settings.ListStyle++; break;
                        }
                    }

                    if (Settings.ExitCombo > 3)
                        Settings.ExitCombo = 0;
                    else if (Settings.ExitCombo < 0)
                        Settings.ExitCombo = 3;

                    if (Settings.ListStyle > 1)
                        Settings.ListStyle = 0;
                    else if (Settings.ListStyle < 0)
                        Settings.ListStyle = 1;

                    if (ctrl.Pressed(Button.Jump) && Settings.Selection == 4)
                    {
                        Settings.Show = false;
                        listStyle = (ListStyle)Settings.ListStyle;

                        string settingsText = Settings.Fullscreenizer.ToString() + "," +
                            Settings.HideMouse.ToString() + "," +
                            Settings.ExitCombo.ToString() + "," +
                            Settings.ListStyle.ToString();

                        File.WriteAllText(@"./settings.d", settingsText);
                    }
                }
                else if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    for (int i = 0; i < games.Length; i++)
                    {
                        switch (listStyle)
                        {
                            case ListStyle.Xbox: games[i].Update(i - selectedGameIndex); break;
                            case ListStyle.Wii: games[i].Update(i == selectedGameIndex, 0); break;
                        }
                    }

                    if (selectedGame == null)
                    {
                        if (alpha < 1)
                            alpha += 0.05f;

                        if (ctrl.Held(Button.Left))
                            scrollBuffer--;
                        else if (ctrl.Held(Button.Right))
                            scrollBuffer++;
                        else
                            scrollBuffer = 0;

                        if (ctrl.Pressed(Button.Left) || (scrollBuffer < -50 && scrollBuffer % 2 == 0))
                        {
                            selectedGameIndex--;
                            if (selectedGameIndex < 0)
                                selectedGameIndex = games.Length - 1;
                            Audio.PlaySound("switch");
                        }
                        if (ctrl.Pressed(Button.Right) || (scrollBuffer > 50 && scrollBuffer % 2 == 0))
                        {
                            selectedGameIndex++;
                            if (selectedGameIndex > games.Length - 1)
                                selectedGameIndex = 0;
                            Audio.PlaySound("switch");
                        }
                        if (listStyle == ListStyle.Wii && ctrl.Pressed(Button.Down))
                        {
                            if (selectedGameIndex + 4 > games.Length - 1)
                                selectedGameIndex = games.Length - 1;
                            else
                                selectedGameIndex += 4;
                            Audio.PlaySound("switch");
                        }
                        if (listStyle == ListStyle.Wii && ctrl.Pressed(Button.Up))
                        {
                            if (selectedGameIndex < 4)
                                selectedGameIndex = 0;
                            else
                                selectedGameIndex -= 4;
                            Audio.PlaySound("switch");
                        }
                        if (ctrl.Pressed(Button.Jump))
                        {
                            selectedGame = games[selectedGameIndex];
                            Audio.PlaySound("select");
                        }
                        if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed)
                        {
                            Settings.Show = true;
                            Settings.Selection = 0;
                        }
                    }
                    else
                    {
                        if (alpha > 0)
                        {
                            alpha -= 0.05f;
                        }
                        else
                        {
                            RunGame(selectedGame);
                        }
                    }
                }
                else
                {
                    alpha = 1;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(buffer);
            GraphicsDevice.Clear(clearColor);

            spriteBatch.Begin();

            if (background != null)
                spriteBatch.Draw(background, new Rectangle(0, 0, 1600, 960), Color.White * alpha);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                var textX = 800 - font.MeasureString(errorMessage).X / 2;
                spriteBatch.DrawString(font, errorMessage, new Vector2(textX, 360), Color.Black);

                if (exitCount > 0)
                    spriteBatch.DrawString(font, "Hold to Quit...", new Vector2(400, 16), Color.Black);
            }
            else
            {
                if (!isGameRunning)
                {
                    if (Settings.Show)
                    {
                        spriteBatch.DrawString(font, "Fullscreenizer:", new Vector2(250, 150), Settings.Selection == 0 ? Color.Black : Color.Gray);
                        spriteBatch.DrawString(font, "Hide Mouse:", new Vector2(250, 250), Settings.Selection == 1 ? Color.Black : Color.Gray);
                        spriteBatch.DrawString(font, "Exit Combo:", new Vector2(250, 350), Settings.Selection == 2 ? Color.Black : Color.Gray);
                        spriteBatch.DrawString(font, "List Style:", new Vector2(250, 450), Settings.Selection == 3 ? Color.Black : Color.Gray);
                        spriteBatch.DrawString(font, "Save and Return", new Vector2(550, 650), Settings.Selection == 4 ? Color.Black : Color.Gray);

                        spriteBatch.DrawString(font, Settings.Fullscreenizer ? "ON" : "OFF", new Vector2(900, 150), Settings.Selection == 0 ? Color.Black : Color.Gray);
                        spriteBatch.DrawString(font, Settings.HideMouse ? "ON" : "OFF", new Vector2(900, 250), Settings.Selection == 1 ? Color.Black : Color.Gray);
                        switch (Settings.ExitCombo)
                        {
                            case 0:
                                spriteBatch.DrawString(font, "R3 + L3", new Vector2(900, 350), Settings.Selection == 2 ? Color.Black : Color.Gray);
                                break;
                            case 1:
                                spriteBatch.DrawString(font, "Start + Back", new Vector2(900, 350), Settings.Selection == 2 ? Color.Black : Color.Gray);
                                break;
                            case 2:
                                spriteBatch.DrawString(font, "Hold Start", new Vector2(900, 350), Settings.Selection == 2 ? Color.Black : Color.Gray);
                                break;
                            case 3:
                                spriteBatch.DrawString(font, "Hold Back", new Vector2(900, 350), Settings.Selection == 2 ? Color.Black : Color.Gray);
                                break;
                        }
                        switch (Settings.ListStyle)
                        {
                            case 0:
                                spriteBatch.DrawString(font, "XBox", new Vector2(900, 450), Settings.Selection == 3 ? Color.Black : Color.Gray);
                                break;
                            case 1:
                                spriteBatch.DrawString(font, "Wii", new Vector2(900, 450), Settings.Selection == 3 ? Color.Black : Color.Gray);
                                break;
                        }
                    }
                    else if (listStyle == ListStyle.Xbox)
                    {
                        for (int i = games.Length - 1; i > selectedGameIndex; i--)
                        {
                            spriteBatch.Draw(games[i].ImageTexture,
                            games[i].Position, new Rectangle(0, 0, games[i].ImageTexture.Width, games[i].ImageTexture.Height),
                            Color.White, 0, new Vector2(games[i].ImageTexture.Width / 2, games[i].ImageTexture.Height / 2), SpriteEffects.None, 0);
                        }

                        for (int i = 0; i < selectedGameIndex; i++)
                        {
                            spriteBatch.Draw(games[i].ImageTexture,
                            games[i].Position, new Rectangle(0, 0, games[i].ImageTexture.Width, games[i].ImageTexture.Height),
                            Color.White, 0, new Vector2(games[i].ImageTexture.Width / 2, games[i].ImageTexture.Height / 2), SpriteEffects.None, 0);
                        }

                        spriteBatch.Draw(games[selectedGameIndex].ImageTexture,
                            games[selectedGameIndex].Position, new Rectangle(0, 0, games[selectedGameIndex].ImageTexture.Width, games[selectedGameIndex].ImageTexture.Height),
                            Color.White, 0, new Vector2(games[selectedGameIndex].ImageTexture.Width / 2, games[selectedGameIndex].ImageTexture.Height / 2), SpriteEffects.None, 0);

                        var textX = 800 - font.MeasureString(games[selectedGameIndex].Name).X / 2;
                        spriteBatch.DrawString(font, games[selectedGameIndex].Name, new Vector2(textX, 720), Color.Black);
                    }
                    else if (listStyle == ListStyle.Wii)
                    {
                        int column = 0;
                        int row = 0;
                        int page = 0;
                        int curPage = selectedGameIndex / 12;
                        scrollX = (scrollX + curPage * 1600) / 2;

                        for (int i = 0; i < games.Length; i++)
                        {
                            spriteBatch.Draw(games[i].ImageTexture,
                            new Rectangle(204 + column * 400 + ((page * 1600) - scrollX), 160 + row * 280, games[i].Position.Width, games[i].Position.Height),
                            new Rectangle(0, 0, games[i].ImageTexture.Width, games[i].ImageTexture.Height),
                            Color.White, 0, new Vector2(games[i].ImageTexture.Width / 2, games[i].ImageTexture.Height / 2), SpriteEffects.None, 0);

                            column++;
                            if (column >= 4)
                            {
                                column = 0;
                                row++;
                            }
                            if (row >= 3)
                            {
                                row = 0;
                                page++;
                            }
                        }

                        spriteBatch.DrawString(font, games[selectedGameIndex].Name, new Vector2(700, 900), Color.Black);
                    }

                    if (exitCount > 0)
                        spriteBatch.DrawString(font, "Hold to Quit...", new Vector2(400, 16), Color.Black);

                    spriteBatch.DrawString(font, "(Y) Settings", new Vector2(100, 900), Color.Black);
                }
            }

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(buffer, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White * alpha);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadGameList()
        {
            List<GameItem> gameList = new List<GameItem>();

            if (Directory.Exists(gameDirectory))
            {
                gameList = GetGamesFromDir(gameDirectory, gameList);

                string[] innerDirectories = Directory.GetDirectories(gameDirectory);

                foreach (string dir in innerDirectories)
                {
                    gameList = GetGamesFromDir(dir, gameList);
                }
            }

            games = gameList.ToArray();
        }

        private List<GameItem> GetGamesFromDir(string directory, List<GameItem> gameList)
        {
            string[] dirFiles = Directory.GetFiles(directory);

            foreach (string file in dirFiles)
            {
                foreach (string extension in gameExtensions)
                {
                    if (extension.ToLower().Equals(file.Substring(file.Length - 3).ToLower()))
                    {
                        GameItem newGame = new GameItem();
                        newGame.Path = file;
                        newGame.Name = file.Replace(directory, "").Replace(@"\", "").Replace("." + extension, "");

                        foreach (string imageExt in imageExtensions)
                        {
                            string fileImageExt = file.Substring(0, file.Length - 3) + imageExt;

                            if (File.Exists(fileImageExt))
                            {
                                newGame.ImagePath = fileImageExt;
                            }
                        }

                        foreach (string amFile in dirFiles)
                        {
                            if (amFile.Contains(newGame.Name) && amFile.Contains(".amgp"))
                            {
                                newGame.IncludeKeyboard = true;
                                newGame.KeyboardProfile = amFile;
                            }
                        }

                        foreach (string exempt in fullscreenExemptions)
                        {
                            if (exempt.Equals(newGame.Name))
                                newGame.FullscreenExempt = true;
                        }

                        foreach (string exclude in excludedGames)
                        {
                            if (exclude.Equals(newGame.Name))
                                newGame.Excluded = true;
                        }

                        try
                        {
                            newGame.ImageTexture = LoadImageFromFile(newGame.ImagePath);
                        }
                        catch
                        {
                            try
                            {
                                newGame.ImageTexture = LoadImageFromFile("d_icon.png");
                            }
                            catch
                            {
                                newGame.ImageTexture = new Texture2D(GraphicsDevice, 320, 200);
                            }
                        }

                        if (!newGame.Excluded)
                            gameList.Add(newGame);
                    }
                }
            }

            return gameList;
        }

        private void RunGame(GameItem game)
        {
            if (game != null)
            {
                isGameRunning = true;
                fullscreenCount = 100;

                if (game.IncludeKeyboard)
                {
                    ProcessStartInfo antiMicroInfo = new ProcessStartInfo(@"C:\Users\conne\source\repos\IndieGameStation\IndieGameStation\bin\DesktopGL\AnyCPU\Debug\AntiMicro\antimicro.exe");
                    antiMicroInfo.Arguments = "--hidden --profile \"" + game.KeyboardProfile + "\"";
                    antiMicroInfo.UseShellExecute = false;
                    antiMicro = Process.Start(antiMicroInfo);

                    ProcessStartInfo startInfo = new ProcessStartInfo(game.Path);
                    startInfo.WorkingDirectory = Path.GetDirectoryName(game.Path);
                    process = Process.Start(startInfo);
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(game.Path);
                    startInfo.WorkingDirectory = Path.GetDirectoryName(game.Path);
                    process = Process.Start(startInfo);
                }
            }
        }

        private Texture2D LoadImageFromFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            Texture2D texture = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();

            return texture;
        }
    }
}
