using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using System;

class VGrid
{
    private int width, height;
    public VGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public List<RectangleShape> Update(byte[] r, string[] color, int psize)
    {
        List<RectangleShape> grid = new List<RectangleShape>();

        for (int h = 0; h < this.height; h++)
        {
            for (int w = 0; w < this.width; w++)
            {
                RectangleShape rect = new RectangleShape(new Vector2f(psize, psize));
                rect.Origin = 0f * rect.Size;
                rect.Position = new Vector2f(w * 10, h * 10);

                var pos = h*this.width+w;
                
                var rgb = color[r[pos]].Split(' ');

                rect.FillColor = new Color((byte)Convert.ToInt32(rgb[0]), (byte)Convert.ToInt32(rgb[1]), (byte)Convert.ToInt32(rgb[2]));

                grid.Add(rect);
            }
        }

        return grid;
    }





}