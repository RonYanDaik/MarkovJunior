// Copyright (C) 2022 Maxim Gumin, The MIT License (MIT)

using System;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using SFML.Window;
using SFML.Graphics;
using SFML.System;

static class Program
{
    static void MainOld()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();

        Dictionary<char, int> palette = XDocument.Load("resources/palette.xml").Root.Elements("color").ToDictionary(x => x.Get<char>("symbol"), x => Convert.ToInt32(x.Get<string>("value"), 16) + (255 << 24));

        Random meta = new();
        XDocument xdoc = XDocument.Load("models.xml", LoadOptions.SetLineInfo);
        foreach (XElement xmodel in xdoc.Root.Elements("model"))
        {
            string name = xmodel.Get<string>("name");
            int linearSize = xmodel.Get("size", -1);
            int dimension = xmodel.Get("d", 2);
            int MX = xmodel.Get("length", linearSize);
            int MY = xmodel.Get("width", linearSize);
            int MZ = xmodel.Get("height", dimension == 2 ? 1 : linearSize);

            Console.Write($"{name} > ");
            string filename = $"models/{name}.xml";
            XDocument modeldoc;
            try { modeldoc = XDocument.Load(filename, LoadOptions.SetLineInfo); }
            catch (Exception)
            {
                Console.WriteLine($"ERROR: couldn't open xml file {filename}");
                continue;
            }

            Interpreter interpreter = Interpreter.Load(modeldoc.Root, MX, MY, MZ);
            if (interpreter == null)
            {
                Console.WriteLine("ERROR");
                continue;
            }

            int amount = xmodel.Get("amount", 2);
            int pixelsize = xmodel.Get("pixelsize", 4);
            string seedString = xmodel.Get<string>("seeds", null);
            int[] seeds = seedString?.Split(' ').Select(s => int.Parse(s)).ToArray();
            bool gif = xmodel.Get("gif", false);
            bool iso = xmodel.Get("iso", false);
            int steps = xmodel.Get("steps", gif ? 1000 : 50000);
            int gui = xmodel.Get("gui", 0);
            if (gif) amount = 1;

            for (int k = 0; k < amount; k++)
            {
                int seed = seeds != null && k < seeds.Length ? seeds[k] : meta.Next();
                foreach ((byte[] result, char[] legend, int FX, int FY, int FZ) in interpreter.Run(seed, steps, 5,gif))
                {
                    int[] colors = legend.Select(ch => palette[ch]).ToArray();
                    var counter = interpreter.counter.ToString("00000000.##");

                    string outputname = gif ? $"output/{counter}" : $"output/{name}_{seed}";
                    if (FZ == 1 || iso)
                    {
                        var (bitmap, WIDTH, HEIGHT) = Graphics.Render(result, FX, FY, FZ, colors, pixelsize, gui);
                        if (gui > 0) GUI.Draw(name, interpreter.root, interpreter.current, bitmap, WIDTH, HEIGHT, palette);
                        Graphics.SaveBitmap(bitmap, WIDTH, HEIGHT, outputname + ".png");
                    }
                    else VoxHelper.SaveVox(result, (byte)FX, (byte)FY, (byte)FZ, colors, outputname + ".vox");
                }
                Console.WriteLine("DONE");
            }
        }
        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }

    static void Main()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();

        Dictionary<char, int> palette = XDocument.Load("resources/palette.xml").Root.Elements("color").ToDictionary(x => x.Get<char>("symbol"), x => Convert.ToInt32(x.Get<string>("value"), 16) + (255 << 24));
        Dictionary<char, string> rgbpalette = XDocument.Load("resources/palette.xml").Root.Elements("color").ToDictionary(x => x.Get<char>("symbol"), x => x.Get<string>("rgb"));

        Random meta = new();
        XDocument xdoc = XDocument.Load("models.xml", LoadOptions.SetLineInfo);
        foreach (XElement xmodel in xdoc.Root.Elements("model"))
        {
            string name = xmodel.Get<string>("name");
            int linearSize = xmodel.Get("size", -1);
            int dimension = xmodel.Get("d", 2);
            int MX = xmodel.Get("length", linearSize);
            int MY = xmodel.Get("width", linearSize);
            int MZ = xmodel.Get("height", dimension == 2 ? 1 : linearSize);

            Console.Write($"{name} > ");
            string filename = $"models/{name}.xml";
            XDocument modeldoc;
            try { modeldoc = XDocument.Load(filename, LoadOptions.SetLineInfo); }
            catch (Exception)
            {
                Console.WriteLine($"ERROR: couldn't open xml file {filename}");
                continue;
            }

            Interpreter interpreter = Interpreter.Load(modeldoc.Root, MX, MY, MZ);
            if (interpreter == null)
            {
                Console.WriteLine("ERROR");
                continue;
            }

            int amount = xmodel.Get("amount", 2);
            int pixelsize = xmodel.Get("pixelsize", 10);
            string seedString = xmodel.Get<string>("seeds", null);
            int[] seeds = seedString?.Split(' ').Select(s => int.Parse(s)).ToArray();
            bool gif = xmodel.Get("gif", false);
            bool iso = xmodel.Get("iso", false);
            int maxSteps = xmodel.Get("steps", gif ? 1000 : 50000);
            int gui = xmodel.Get("gui", 0);
            if (gif) amount = 1;








            // create the window.
            RenderWindow window = new RenderWindow(new VideoMode(1000, 1000), "Hello SFML.Net!", Styles.Close, new ContextSettings(24, 8, 2));
            window.SetVerticalSyncEnabled(true);
            window.SetActive();

            // Setup event handlers
            window.Closed += new EventHandler(OnClosed);
            window.KeyPressed += new EventHandler<SFML.Window.KeyEventArgs>(OnKeyPressed);


            // Set up timing
            Clock clock = new Clock();

            var grid = new VGrid(MX, MY);
            // var grid = new VGrid(3, 3);

            while (window.IsOpen)
            {

                for (int k = 0; k < amount; k++)
                {

                    int seed = seeds != null && k < seeds.Length ? seeds[k] : meta.Next();
                    foreach ((byte[] result, char[] legend, int FX, int FY, int FZ) in interpreter.Run(seed, maxSteps, 2, gif))
                    {
                        window.SetFramerateLimit(10);
                        // Update objects
                        window.DispatchEvents();

                        // Display objects
                        window.Clear(new Color((byte)50, (byte)50, (byte)180));

                        List<RectangleShape> rects = new List<RectangleShape>();

                        string[] colors = legend.Select(ch => rgbpalette[ch]).ToArray();
                        var counter = interpreter.counter.ToString("00000000.##");

                        string outputname = gif ? $"output/{counter}" : $"output/{name}_{seed}";
                        if (FZ == 1 || iso)
                        {
                            // var (bitmap, WIDTH, HEIGHT) = Graphics.Render(result, FX, FY, FZ, colors, pixelsize, gui);
                            // if (gui > 0) GUI.Draw(name, interpreter.root, interpreter.current, bitmap, WIDTH, HEIGHT, palette);
                            //Graphics.SaveBitmap(bitmap, WIDTH, HEIGHT, outputname + ".png");
                            rects = grid.Update(result, colors, pixelsize);
                        }
                        // else VoxHelper.SaveVox(result, (byte)FX, (byte)FY, (byte)FZ, colors, outputname + ".vox");

                        foreach (var rect in rects)
                        {
                            window.Draw(rect);
                        }
                        window.Display();
                    }
                    Console.WriteLine("DONE");
                }



                // System.Threading.Thread.Sleep(100);
                // // Update objects
                // window.DispatchEvents();

                // // Display objects
                // window.Clear(new Color((byte)50, (byte)50, (byte)180));
                // byte[] result = new byte[] { 0, 1, 1, 1, 0, 1, 1, 1, 0 };
                // int[] colors = new int[] { 1666666, 6666666 };
                // List<RectangleShape> rects = new List<RectangleShape>();
                // rects = grid.Update(result, colors, pixelsize);

                // foreach (var rect in rects)
                // {
                //     window.Draw(rect);
                // }
                // window.Display();

            }
        }
    }

    static void OnClosed(object sender, EventArgs e)
    {
        RenderWindow window = (RenderWindow)sender;
        window.Close();
    }
    static void OnKeyPressed(object sender, KeyEventArgs e)
    {
        RenderWindow window = (RenderWindow)sender;
        if (e.Code == Keyboard.Key.Escape)
            window.Close();
    }

}
