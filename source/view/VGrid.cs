using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using System;

class VGrid
{
    public int width, height;
    public VGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
    public struct gui_setup
    {
        public int gui;

    };

    public List<RectangleShape> Update(byte[] rslt, string[] color, int psize , gui_setup gui)
    {
        List<RectangleShape> grid = new List<RectangleShape>();
        Random r = new Random();

        for (int h = 0; h < this.height; h++)
        {
            for (int w = 0; w < this.width; w++)
            {
                RectangleShape rect = new RectangleShape(new Vector2f(psize, psize));
                rect.Origin = 0f * rect.Size;
                rect.Position = new Vector2f(w * 10, h * 10);

                var pos = h*this.width + w;
                if(pos>rslt.Length)
                    continue;
                if(rslt[pos]>color.Length)
                {
                    
                    rect.FillColor = new Color(255,255,255 );
                }
                else{
                    var rgb = color[rslt[pos]].Split(' ');
                    rect.FillColor = new Color((byte)Convert.ToInt32(rgb[0]), (byte)Convert.ToInt32(rgb[1]), (byte)Convert.ToInt32(rgb[2]));
                }
                grid.Add(rect);
            }
        }


        return grid;
    }





}