# SpecCore

This is a simple C# WPF "game engine" inspired by many happy hours spent gaming and programming on my Sinclair ZX Spectrum, in the mid-80s. The 640x320 screen, and blocky 7-colour graphics had a charm of their own. The engine is very simple, and has been loosely based on the Sinclair Basic programming syntax, as has the 60x40 character screen.

The project is just a bit of fun, which I hope brings a smile to the face of 80s kids like me, looking for a bit of nostalgia.

## Getting Started

Download the source and compile it. You'll find a few basic examples that you can try - edit MainWindow.xaml.cs code-behind, and uncomment one of the lines *only* in the constructor.

To write your own game or program, add a class that inherits from SpeccyEngine. Implement the necessary constructor, and override the Init() and DoFrame() methods. The class should look something like this:-

    public class HelloWorld : SpeccyEngine
    {
        public HelloWorld(Window window)
            : base(window)
        {
        }
    }

Override the Init() method. This is called once when your application first runs, allowing you to perform any necessary initialisation:-

    protected override void Init()
    {
        Clear(Colors.Black); // Clear the screen
        
        Paper = Colors.Yellow; // Set the text foreground and background
        Pen = Colors.Blue;
        
        Print(5, 24, "Hello World!"); // Display some text
    }

Now override the DoFrame() method (note the "async" keyword). This is called several times per second, and is where you animate your game - think of it as rendering a single "frame". You might respond to a keypress, move your spaceship one column to the right, move the advancing invaders down one row, and so on:-

    protected override async void DoFrame()
    {
        // Draw a "*" at a random position whenever SPACE is pressed.
        if (LastKeyPress == Key.Space) 
        {
            Paper = Colors.Black;
            Pen = Colors.Red;

            var r = new Random();
            Print(r.Next(60), r.Next(40), "*");
        }
    }

Finally,  you need to wire it up to the UI. Open up the main window code-behind (MainWindow.xaml.cs) and add this line to the constructor, immediately before *InitializeComponent()*:-

    new HelloWorld(this);

And that's it! Run the app and you should see our "Hello World!" message, and random asterisks that appear each time you hit the space bar. The engine also lets you resize the window using F6 & F7.


## Authors

* **Andrew Stephens**

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Sir Clive Sinclair & Sinclair Research, for giving me the programming bug in the early 80s, setting me on the road to a career in software development
