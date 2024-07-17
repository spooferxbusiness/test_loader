using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyAuth;

// Before you look at this source and think "Oh yes it's another weird comm source" - you are right, but I had 1 goal during writing this - 
// USE KEYAUTH ONLY, without any middleman server - of course it's hard to have a secure loader especially when you have your auth (data) directly in your app.

// If you're using KeyAuth and need a secure loader, remember what a loader is. It loads a cheat. That's all. You do NOT need all of this antidebug bollocks
// Just remember to have a secure server side with session checks to verify a user is logged in. Have your module encrypted on your server and decrypt-
// at runtime. Make sure to require auth validation when requesting the module. KeyAuth does this well.

// Another tip is: do not fight people trying to abuse your loader by trying to jmp, nop your auth inputs, because your loader has an auth and needs a validated-
// session to actually request X module.. So what do they get?? They get a loader that doesn't do its job.

// Yes this source is somewhat pasted, like little functs in misc, then some are messy and not efficient at all. I did this because there's no need.





// SETUP GUIDE: Write your auth vars like so: 
/*
 
        Console.WriteLine(misc.Encrypt("name", loader_fra.Properties.Resources.name_key));
        Console.WriteLine(misc.Encrypt("ownerid", loader_fra.Properties.Resources.ownerid_key));
        Console.WriteLine(misc.Encrypt("secret", loader_fra.Properties.Resources.secret_key));
        Console.WriteLine(misc.Encrypt("version", loader_fra.Properties.Resources.version_key));




        To encrypt your file, just call

        misc.XorEncrypt(auth.Download(""), auth.var("module_decryption_key")); 

        and write it to disk, then upload to keyauth.

 */

// This will give you the encrypted variables, the project's resources store each key, change them.
// Change the auth_data array to the encrypted vars, order: name,ownerid,secret,version

// To decrypt them, just call
/*
 
    string[] auth_dec = misc.DAD(auth_data);

 */

// this will get you the decrypted vars to connect with, etc. Only do this when you NEED them, otherwise you can call misc.ClearArray(auth_dec);


class Program
{

    [DllImport("libs\\Fart.png", CharSet = CharSet.Unicode)]
    public static extern bool mapDrv(byte[] driver); // kdmapper.. Literally just kdmapper.. This is a test, not supposed to be distributed. IT'S RECOMMENDED TO ADD AUTH TO THIS DLL AND CHECK IF SESSION VALID.

    [DllImport("libs\\DogeCoinMiner.png", CharSet = CharSet.Unicode)]
    public static extern void init(); // Shit protection lib.

    public static string[] auth_data = {
        "FhQ4Fg==",
        "RnFqMRdaHSEXJg==",
        "QF8gRGF4ABBwCBUECwp2WAU/U1pIDUotQQBYYCEvYCVFWSAQMHkESnQNQw8KWC4NCjsADEhYHXpAVl83dSs0cQ==",
        "AUdl",
    
    }; // name,ownerid,secret,version

    public static int flags = 0;
    public static bool isLoggedIn = false;
    public static bool expiredSession = false;
    public static bool offlineApp = false;

    static void Main()
    {
        if (Debugger.IsAttached || Debugger.IsLogging()) { Debugger.Break(); Debugger.Log(3, "Error", "%s%s%s%s%s%s%s%s%s%s%s%s%s"); Environment.Exit(0); }

        init();

        Task.Run(async () => { await Task.Delay(120000); expiredSession = true;  MessageBox.Show("Session expired. Please restart."); Environment.Exit(0); }); // 1 min

        string[] AUT = misc.DAD(auth_data);

        api auth = new api(AUT[0], AUT[1], AUT[2], AUT[3]);


        ClearArray(ref AUT);

        // Lambda function to check response
        Func<bool, bool> succeeded = (bool type) =>
        {
            if (type == false)
            {
                auth.CheckInit();

                if (auth.response.success)
                {
                    if (auth.checkblack())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                auth.check();

                if (auth.response.success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        };


        Console.Title = string.Empty;
        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine("loader> connecting..");

        AUT = misc.DAD(auth_data);

        auth.init();

        if (!succeeded(false))
        {
            ClearArray(ref AUT);
            Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
            MessageBox.Show(auth.response.message);
            Environment.Exit(0);
        }
        else
        {
            //ClearArray(ref AUT);
            flags++;

            if (auth.var("loader_status") == "offline")
            {
                offlineApp = true;
                MessageBox.Show("Application is currently offline. Please check the Discord for updates.");
                Environment.Exit(0);
            }
            else if (auth.var("loader_status") == "online")
            {
                if (expiredSession || offlineApp)
                {
                    Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                    MessageBox.Show("Session expired or application is offline. Please try again later.");
                    Environment.Exit(0);
                }

                Console.WriteLine("loader> checking environment..");

                var fart_hash = auth.var("fart_hash");
                var dcm_hash = auth.var("dcm_hash");

                if (misc.getMD5Hash(Directory.GetCurrentDirectory() + "\\libs\\Fart.png") != fart_hash || misc.getMD5Hash(Directory.GetCurrentDirectory() + "\\libs\\DogeCoinMiner.png") != dcm_hash)
                {
                    ClearArray(ref AUT);
                    Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                    MessageBox.Show("Hash check failed for libs.");
                    Environment.Exit(0);
                }

                Console.WriteLine("loader> loading...");
                Thread.Sleep(2500);

                Console.Clear();



                var license = "";
                if (File.Exists(Directory.GetCurrentDirectory() + "\\license.txt"))
                {
                    license = File.ReadAllText(Directory.GetCurrentDirectory() + "\\license.txt");
                    Console.WriteLine("loader> logging in via file");
                }
                else
                {
                    Console.Write("loader> enter license key: ");
                    license = Console.ReadLine().Replace("\"", string.Empty).Replace(" ", string.Empty);
                }

                auth.license(license);

                if (auth.response.success)
                {
                    if (succeeded(true))
                    {
                        if (!File.Exists(Directory.GetCurrentDirectory() + "\\license.txt"))
                        {
                            File.WriteAllText(Directory.GetCurrentDirectory() + "\\license.txt", license);
                        }

                        isLoggedIn = true;
                        flags++;

                        if (expiredSession || offlineApp)
                        {
                            Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                            MessageBox.Show("Session expired or application is offline. Please try again later.");
                            Environment.Exit(0);
                        }
                        Console.WriteLine("loader> product status: " + auth.var("status"));

                        for (var i = 0; i < auth.user_data.subscriptions.Count; i++)
                        {
                            int hours = Convert.ToInt32(auth.user_data.subscriptions[i].timeleft) / 3600;
                            Console.WriteLine("loader> expires in " + Convert.ToString(hours) + "h");
                        }


                        Thread.Sleep(4500);
                        Console.Clear();


                        if (flags == 2 && isLoggedIn == true)
                        {
                            Console.WriteLine("loader> streaming..");
                            byte[] module = auth.download("211083");

                            if (module != null)
                            {
                                Console.WriteLine("loader> streaming succeeded.");


                                //Console.WriteLine("debug> decrypting module..");
                                string key = auth.var("module_decryption_key");
                                //Console.WriteLine("debug> decryption key: " + key);

                                byte[] decrypted_data = misc.XorDecrypt(module, key);

                                Console.WriteLine("loader> press enter to map."); // or map in game, etc,
                                Console.ReadLine();
                                Console.WriteLine();

                                if (mapDrv(decrypted_data)) // could add auth to mapper but i'm a lazy cunt and this is just a test
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("loader> mapped driver.");
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("loader> failed to map driver.");
                                }

                                misc.ClearString(ref key);
                                misc.ClearBytes(ref decrypted_data);
                                misc.ClearBytes(ref module);

                                Console.WriteLine();
                                Thread.Sleep(2500);




                            }
                            else
                            {
                                Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                                MessageBox.Show(auth.response.message);
                                Environment.Exit(0);
                            }

                        }
                        else
                        {
                            Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                            MessageBox.Show("Flags mismatched or failed to login.");
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                        MessageBox.Show(auth.response.message);
                        Environment.Exit(0);
                    }

                }
                else
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + "\\license.txt"))
                    {
                        File.Delete(Directory.GetCurrentDirectory() + "\\license.txt");
                    }

                    Task.Run(async () => { await Task.Delay(1900); Environment.Exit(0); });
                    MessageBox.Show(auth.response.message);
                    Environment.Exit(0);
                }
            }
            else
            {
                Task.Run(async () => { await Task.Delay(2500); Environment.Exit(0); });
                MessageBox.Show("Error when finding loader status.");
                Environment.Exit(0);

            }
        }

         

        ClearArray(ref AUT);
        Task.Run(async () => { await Task.Delay(2500); Environment.Exit(0); }); // incase gets hooked, etc -- we want this to shut as soon as possible
        Console.WriteLine();
        Console.Clear();
        Console.WriteLine("loader> application finished, exiting..");
        Thread.Sleep(2500);
        Environment.Exit(0);

        
    }

    public static void ClearArray(ref string[] S)
    {
        for (int i = 0; i < S.Length; i++)
        {
            misc.ClearString(ref S[i]);
        }

    }
}
