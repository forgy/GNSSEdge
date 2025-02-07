namespace NtripShare.GNSS.Device.Helper
{
    public class RunCommand
    {
        public RunCommand(string fileName, string arguments, string workingDirectory = "")
        {
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }

        public string Execute(int milliseconds = 10000)
        {
            var rValue = "";
            using (System.Diagnostics.Process p = new System.Diagnostics.Process())
            {
                p.StartInfo.FileName = FileName;
                p.StartInfo.Arguments = Arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WorkingDirectory = WorkingDirectory;
                p.Start();
                p.WaitForExit(milliseconds);
                rValue = p.StandardOutput.ReadToEnd();
            }
            return rValue;
        }
    }
}
