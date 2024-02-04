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
    static bool close = false;
    static int next_model = 0;
    static int next = 0;
    static int current_model = 0;
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
                        //if (gui > 0) GUI.Draw(name, interpreter.root, interpreter.current, bitmap, WIDTH, HEIGHT, palette);
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
        
        RenderWindow window = new RenderWindow(new VideoMode(1000, 1000), "Hello SFML.Net!", Styles.Close, new ContextSettings(24, 8, 2));

        IEnumerable<XElement> xmodelE = xdoc.Root.Elements("model");
        //foreach (XElement xmodel in xdoc.Root.Elements("model"))
        next_model = 0;
        current_model = -1;

         // create the window.
        window.SetVerticalSyncEnabled(true);
        window.SetActive();

        // Setup event handlers
        window.Closed += new EventHandler(OnClosed);
        window.KeyPressed += new EventHandler<SFML.Window.KeyEventArgs>(OnKeyPressed);
        window.MouseButtonPressed += new EventHandler<SFML.Window.MouseButtonEventArgs>(OnMousePressed);

        window.SetFramerateLimit(1);
        // Update objects

        // Display objects
       
       next = 1;
        
        window.Clear(new Color((byte)50, (byte)50, (byte)50));
        
        RenderData rd = new RenderData();
        while (window.IsOpen)
        {
            window.DispatchEvents();
            window.Display();

            if (next!=0 && current_model < xmodelE.Count()) 
            {
                next = 0;
                current_model ++;
                //for(current_model = 0; current_model < xmodelE.Count(); current_model++) 
                {   

                    if(current_model<0)
                        current_model = xmodelE.Count()-1;
                    current_model = current_model%xmodelE.Count();
                    XElement xmodel = xmodelE.ElementAt(current_model);
                    
                    if(close)
                        break;

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
                    interpreter.modelName = name;
                    
                    if (interpreter == null)
                    {
                        Console.WriteLine("ERROR");
                        continue;
                    }

                    int amount = xmodel.Get("amount", 2);
                    int pixelsize = xmodel.Get("pixelsize", 10);
                    string seedString = xmodel.Get<string>("seeds", null);
                    int[] seeds = seedString?.Split(' ').Select(s => int.Parse(s)).ToArray();
                    bool gif = xmodel.Get("gif", true);
                    bool iso = xmodel.Get("iso", true);
                    int maxSteps = xmodel.Get("steps", gif ? 5000 : 50000);
                    int gui = xmodel.Get("gui", 120);
                    
                    if (gif) 
                        amount = 1;

                    // Set up timing
                    Clock clock = new Clock();

                    var grid = new VGrid(MX, MY);
                    byte[] pixels = new byte[MX * MY * 4];
                    Texture tex = new Texture((uint)MX,(uint)MY);
                    
                    //Image sfml_image;

                    SFML.Graphics.Sprite sprite = new SFML.Graphics.Sprite(tex);
                    sprite.Texture = tex;

                    rd.xmodel = xmodel;
                    rd.palette = palette;
                    rd.amount = amount;
                    rd.seeds = seeds;
                    rd.meta = meta;
                    rd.interpreter = interpreter;
                    rd.maxSteps = maxSteps;
                    rd.gif = gif;
                    rd.rgbpalette = rgbpalette;
                    rd.name = name;
                    rd.iso = iso;
                    rd.gui = gui;
                    rd.pixels = pixels;
                    rd.tex = tex;
                    rd.pixelsize = pixelsize;
                    rd.sprite = sprite;
                    R(rd,window);
                }
            }
        }
    }
    class RenderData{
        public SFML.Graphics.Sprite sprite;
        public int pixelsize;
        public XElement xmodel; 
        public Dictionary<char, int> palette;
        public int amount;

        public int[] seeds;

        public Random meta;

        public Interpreter interpreter;

        public int maxSteps;

        public bool gif;

        public Dictionary<char, string> rgbpalette;

        public string name;

        public int gui;

        public bool iso;

        public byte[] pixels;
        public      Texture tex;
    };

    static void R(RenderData rd, RenderWindow window)
    {
        {
            Dictionary<char, int> customPalette = new(rd.palette);
            foreach (var x in rd.xmodel.Elements("color")) customPalette[x.Get<char>("symbol")] = (255 << 24) + Convert.ToInt32(x.Get<string>("value"), 16);

            for (int k = 0; k < rd.amount; k++) {
                
                if(close)
                    return;

                int seed = rd.seeds != null && k < rd.seeds.Length ? rd.seeds[k] : rd.meta.Next();
                foreach ((byte[] result, char[] legend, int FX, int FY, int FZ) in rd.interpreter.Run(seed, rd.maxSteps, 2, rd.gif)) {
                    
                    if (next>0) {
                        if(next_model<0) {
                            next_model=0;
                            current_model-=2;
                            return;
                        }
                        return;
                    }

                    
                    
                    if(close)
                        return;
                    
                    window.SetFramerateLimit(25);
                    // Update objects
                    window.DispatchEvents();

                    // Display objects
                    window.Clear(new Color((byte)50, (byte)50, (byte)50));

                    List<RectangleShape> rects = new List<RectangleShape>();
                    
                    int[] colors2 = legend.Select(ch => customPalette[ch]).ToArray();

                    string[] colors = legend.Select(ch => rd.rgbpalette[ch]).ToArray();

                    var counter = rd.interpreter.counter.ToString("00000000.##");

                    string outputname = rd.gif ? $"output/{counter}" : $"output/{rd.name}_{seed}";
                    
                    if (FZ == 1 || rd.iso)
                    {
                        var (bitmap, WIDTH, HEIGHT) = Graphics.Render(result, FX, FY, FZ, colors2, rd.pixelsize, rd.gui);
                        if (rd.gui > 0) 
                            GUI.Draw(rd.name, rd.interpreter.root, rd.interpreter.current, bitmap, WIDTH, HEIGHT, rd.palette);
                        
                        //Graphics.SaveBitmap(bitmap, WIDTH, HEIGHT, outputname + ".png");
                        
                        if(rd.pixels.Length!=(WIDTH * HEIGHT * 4)){
                            rd.tex = new Texture((uint)WIDTH,(uint)HEIGHT);
                            rd.pixels = new byte[WIDTH * HEIGHT * 4];
                        }
                        
                        int mask = 0x00_FF;
                        // * 4 because pixels have 4 components (RGBA)
                        for (int h = 0; h < HEIGHT; h++)
                        {
                            for (int w = 0; w < WIDTH; w++)
                            {
                                int col = (int)bitmap[h*WIDTH + w];
                                
                                int aval = (mask) & (int)(col >> 24) ;
                                //aval = aval;
                                int rval = (mask) & (int)(col >>16 );
                                //rval = rval;
                                int gval = (mask) & (int)(col>>8 );
                                //gval = gval;
                                int bval = (mask) & (int)(col);
                                //bval = bval>>0;
                                
                                rd.pixels[(h* WIDTH + w)*4 + 0] = (byte)rval;
                                rd.pixels[(h* WIDTH + w)*4 + 1] = (byte)gval;
                                rd.pixels[(h* WIDTH + w)*4 + 2] = (byte)bval;
                                rd.pixels[(h* WIDTH + w)*4 + 3] = (byte)aval;

                            }
                        }
                        rd.tex.Update(rd.pixels);
                        rd.sprite.Texture = rd.tex;
                        Console.WriteLine($"WIDTH,HEIGHT = ({WIDTH}, {HEIGHT}) | {window.Size}");
                        if((WIDTH )>window.Size.X || (HEIGHT)>window.Size.Y)
                        {
                            int w = Math.Min(WIDTH,1200);
                            int h = Math.Min(HEIGHT,1200);
                            window.Size = new Vector2u((uint) (w),(uint) h);
                            rd.sprite.TextureRect = new IntRect(new Vector2i( 0 , 0),new Vector2i(WIDTH, HEIGHT));

                        }else
                            rd.sprite.TextureRect = new IntRect(new Vector2i( 0 , 0),new Vector2i((int)WIDTH,(int) HEIGHT));
                        
                        window.Draw(rd.sprite);

                    }

                    window.Display();
                    
                }

                Console.WriteLine("DONE");
            }


        }
    }
    static void OnClosed(object sender, EventArgs e)
    {
        RenderWindow window = (RenderWindow)sender;
        window.Close();
        close = true;
    }
    static void OnKeyPressed(object sender, KeyEventArgs e)
    {
        RenderWindow window = (RenderWindow)sender;
        if (e.Code == Keyboard.Key.Escape)
            window.Close();
    }

    static void OnMousePressed(object sender, MouseButtonEventArgs e)
    {
        RenderWindow window = (RenderWindow)sender;
        if (e.Button == Mouse.Button.Left)
        {
            next_model=1;
            next = 1;

        }
        if (e.Button == Mouse.Button.Right)
        {
            next_model=-1;
            next = 1;

        }

        
    }
}
