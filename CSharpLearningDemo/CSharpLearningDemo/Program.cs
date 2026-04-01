namespace CSharpLearningDemo
{
    internal static class Program
    {
        /// <summary>
        ///  Main entry point
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
