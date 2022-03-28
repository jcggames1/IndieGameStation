using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IndieGameStation
{
    public class GameItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IncludeKeyboard { get; set; }
        public string KeyboardProfile { get; set; }
        public bool IncludeMouse { get; set; }
        public string ImagePath { get; set; }
        public bool FullscreenExempt { get; set; }
        public bool Excluded { get; set; }



        public float Scale;
        public Rectangle Position;
        public Texture2D ImageTexture;

        public GameItem()
        {
            Scale = 1;
            Position = new Rectangle(800, 480, 500, 320);
        }

        public GameItem(GraphicsDevice graphics, string name, string path, bool keyboard, string kProfile, bool mouse, string image)
        {
            Name = name;
            Path = path;
            IncludeKeyboard = keyboard;
            KeyboardProfile = kProfile;
            IncludeMouse = mouse;

            Scale = 1;
            Position = new Rectangle(800, 480, 500, 320);

            FileStream fileStream = new FileStream(image, FileMode.Open);
            ImageTexture = Texture2D.FromStream(graphics, fileStream);
            fileStream.Dispose();
        }

        public void Update(int index)
        {
            var xPos = Position.X;
            if (xPos != 800 + index * 150)
                xPos -= (xPos - (800 + index * 150)) / 2;

            var scale = Math.Abs(800 - Position.X) / 4;
            Position = new Rectangle(xPos, 480, 500 - scale, 320 - scale);
        }

        public void Update(bool selected, int page)
        {
            if (selected)
            {
                Position.Width = (Position.Width + 420) / 2;
                Position.Height = (Position.Height + 240) / 2;
            }
            else if (!selected)
            {
                Position.Width = (Position.Width + 340) / 2;
                Position.Height = (Position.Height + 180) / 2;
            }
        }
    }
}
