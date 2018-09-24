using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace CHInjector
{
    public partial class GUI : Form
    {
        private string assemblySubPath = "/Clone Hero_Data/Managed/";
        private readonly string assemblyName = "Assembly-CSharp.dll";
        private readonly string[] injectionPoint = { "MainMenu", "Start" };

        public GUI()
        {
            InitializeComponent();
        }

        private void gamePathBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.pathLabel.Text = fileDialog.SelectedPath;
                this.installBtn.Enabled = true;

                try
                {
                    bool isInjected = IsInjected(pathLabel.Text + assemblySubPath + assemblyName, injectionPoint[0], injectionPoint[1], Directory.GetCurrentDirectory()+ "/CHModloader.dll", "CHModloader.ModLoader", "Init");
                    if (isInjected)
                    {
                        StatusAlreadyInstalled();   
                    }
                    else
                    {
                        StatusReadyToInstall();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private void StatusAlreadyInstalled()
        {
            this.installBtn.Enabled = false;
            this.removeBtn.Enabled = true;
            progressBar.Value = 1;
            progressLabel.Text = "Modloader already installed!";
        }

        private void StatusReadyToInstall()
        {
            this.installBtn.Enabled = true;
            this.removeBtn.Enabled = false;
            progressBar.Value = 0;
            progressLabel.Text = "Modloader ready to install!";
        }

        private void StatusInstalledSuccessfully()
        {
            this.installBtn.Enabled = false;
            this.removeBtn.Enabled = true;
            progressBar.Value = 1;
            progressLabel.Text = "Modloader successfully installed!";
        }

        private void StatusUninstalledSuccessfully()
        {
            this.installBtn.Enabled = true;
            this.removeBtn.Enabled = false;
            progressBar.Value = 0;
            progressLabel.Text = "Modloader successfully uninstalled!";
        }

        private void installBtn_Click(object sender, EventArgs e)
        {
            string mainPath = pathLabel.Text + assemblySubPath;

            // Replace original dll with unobfuscated dll to allow mods
            //File.Copy(mainPath + "/Assembly-CSharp.dll", mainPath + "/Assembly-CSharp.dll.bak", true);
            //File.Copy(Directory.GetCurrentDirectory() + "/Assembly-CSharp.dll", mainPath + "/Assembly-CSharp.dll", true);

            File.Copy(pathLabel.Text + "/Clone Hero.exe", pathLabel.Text + "/Clone Hero.original.exe", true);           
            File.WriteAllBytes(pathLabel.Text + "/Clone Hero.exe", Resource.Clone_Hero);

            File.Copy(Directory.GetCurrentDirectory() + "/CHModloader.dll", mainPath + "CHModloader.dll", true);
            File.Copy(Directory.GetCurrentDirectory() + "/0Harmony.dll", mainPath + "0Harmony.dll", true);

            if (!Directory.Exists(pathLabel.Text + "/Mods"))
            {
                Directory.CreateDirectory(pathLabel.Text + "/Mods");
            }

            try
            {
                Inject(mainPath, assemblyName, injectionPoint[0], injectionPoint[1], "CHModloader.dll", "CHModloader.ModLoader", "Init");
                StatusInstalledSuccessfully();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void Inject(string mainPath, string assemblyToPatch, string assemblyType, string assemblyMethod, string loaderAssembly, string loaderType, string loaderMethod)
        {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(mainPath);

            using (ModuleDefinition
                assembly = ModuleDefinition.ReadModule(mainPath + "/" + assemblyToPatch, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver }),
                loader = ModuleDefinition.ReadModule(mainPath + "/" + loaderAssembly)
            )
            {
                MethodDefinition methodToInject = loader.GetType(loaderType).Methods.Single(x => x.Name == loaderMethod);
                MethodDefinition methodToHook = assembly.GetType(assemblyType).Methods.First(x => x.Name == assemblyMethod);

                Instruction loaderInit = Instruction.Create(OpCodes.Call, assembly.ImportReference(methodToInject));
                ILProcessor processor = methodToHook.Body.GetILProcessor();
                processor.InsertBefore(methodToHook.Body.Instructions[0], loaderInit);

                assembly.Write();
            }
        }

        private bool IsInjected(string assemblyToPatch, string assemblyType, string assemblyMethod, string loaderAssembly, string loaderType, string loaderMethod)
        {
            using (ModuleDefinition
                assembly = ModuleDefinition.ReadModule(assemblyToPatch),
                loader = ModuleDefinition.ReadModule(loaderAssembly)
            )
            {
                MethodDefinition methodToInject = loader.GetType(loaderType).Methods.Single(x => x.Name == loaderMethod);
                MethodDefinition methodToHook = assembly.GetType(assemblyType).Methods.First(x => x.Name == assemblyMethod);

                foreach (Instruction instruction in methodToHook.Body.Instructions)
                {
                    if (instruction.OpCode.Equals(OpCodes.Call) && instruction.Operand.ToString().Equals($"System.Void {loaderType}::{loaderMethod}()"))
                    {
                        return true;
                    }
                }
                return false;
            }
           
        }

        private void Remove(string mainPath, string assemblyToPatch, string assemblyType, string assemblyMethod, string loaderAssembly, string loaderType, string loaderMethod)
        {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(mainPath);

            using (ModuleDefinition
                assembly = ModuleDefinition.ReadModule(mainPath + "/" + assemblyToPatch, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver }),
                loader = ModuleDefinition.ReadModule(mainPath + "/" + loaderAssembly)
            )
            {
                MethodDefinition methodToInject = loader.GetType(loaderType).Methods.Single(x => x.Name == loaderMethod);
                MethodDefinition methodToHook = assembly.GetType(assemblyType).Methods.First(x => x.Name == assemblyMethod);

                Instruction toRemove = null;
                foreach (Instruction instruction in methodToHook.Body.Instructions)
                {
                    if (instruction.OpCode.Equals(OpCodes.Call) && instruction.Operand.ToString().Equals($"System.Void {loaderType}::{loaderMethod}()"))
                    {
                        toRemove = instruction;
                        break;
                    }
                }
                if (toRemove != null)
                {
                    ILProcessor processor = methodToHook.Body.GetILProcessor();
                    processor.Remove(toRemove);
                }

                assembly.Write();
            }
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            string mainPath = pathLabel.Text + assemblySubPath;
            try
            {
                Remove(mainPath, assemblyName, injectionPoint[0], injectionPoint[1], "CHModloader.dll", "CHModloader.ModLoader", "Init");
                StatusUninstalledSuccessfully();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            //File.Copy(mainPath + "/Assembly-CSharp.dll.bak", mainPath + "/Assembly-CSharp.dll", true);
            File.Copy(pathLabel.Text + "/Clone Hero.original.exe", pathLabel.Text + "/Clone Hero.exe", true);
            File.Delete(pathLabel.Text + "Clone Hero.original.exe");
            //File.Delete(mainPath + "Assembly-CSharp.dll.bak");
            File.Delete(mainPath + "/CHModloader.dll");
            File.Delete(mainPath + "/0Harmony.dll");
        }
    }
}
