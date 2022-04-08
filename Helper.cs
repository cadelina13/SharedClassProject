using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedClassProject
{
    public static class Helper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string To3Decimal(this double value)
        {
            var rs = String.Format("{0:#,0.000}", value);
            return rs;
        }

        public static long currentTimeMillis()
        {
            return (long)(DateTime.Now - UnixEpoch).TotalMilliseconds;
        }

        public static bool IsVideoFile(string path)
        {
            string[] mediaExtensions = {
                ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
                ".AVI", ".MP4", ".DIVX", ".WMV", ".WEBM" //etc
            };
            return mediaExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }
        public static bool IsExcelFile(string path)
        {
            string[] mediaExtensions = {
                ".XLS", ".XLSX"//etc
            };
            return mediaExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }
        public static bool IsPictureFile(string path)
        {
            string[] mediaExtensions = {
                ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
            };
            return mediaExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }
        public static bool IsPDFFile(string path)
        {
            string[] mediaExtensions = {
                ".PDF" //etc
            };
            return mediaExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        public static DateTime? ToNullIfTooEarlyForDb(this DateTime date)
        {
            return (date >= (DateTime)SqlDateTime.MinValue) ? date : (DateTime?)null;
        }
        public static string toFriendlyDate(DateTime? date)
        {
            if (date.HasValue) return toFriendlyDate(date.Value);

            return "";
        }
        public static string toFriendlyDate(DateTime date)
        {
            long then = toUnixTime(date);
            long now = toUnixTime(DateTime.Now);
            long seconds = (now - then) / 1000;
            long minutes = seconds / 60;
            long hours = minutes / 60;
            long days = hours / 24;
            string friendly = "";
            long num = 0;
            if (days > 3)
            {
                return date.ToString("MMM dd, yyyy");
            }
            if (days > 0)
            {
                num = days;
                friendly = days + " day";
                if (days == 1)
                {
                    return $"Yesterday: {date.ToString("hh:mm tt")}";
                }
            }
            else if (hours > 0)
            {
                num = hours;
                friendly = hours + " hr";
            }
            else if (minutes > 0)
            {
                num = minutes;
                friendly = minutes + " min";
            }
            else
            {
                friendly = "just now";
                return friendly;
            }
            if (num > 1)
            {
                friendly += "s";
            }
            return friendly.ToLower() + " ago";
        }

        static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            md5Hash.Dispose();
            return sBuilder.ToString();
        }
        public static string toPesoSign(decimal value)
        {
            return "₱" + string.Format("{0:n2}", trimDecimal((double)value, 2));
        }
        public static string toPesoSign(double value)
        {
            return "₱" + string.Format("{0:n2}", trimDecimal(value, 2));
        }
        public static string toPesoSign(double? value)
        {
            return value.HasValue ? "₱" + string.Format("{0:n2}", trimDecimal(value.Value, 2)) : "₱0.00";
        }
        public static string toCommaSeparate(double value)
        {
            return string.Format("{0:n0}", trimDecimal(value, 0));
        }
        static bool VerifyMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static DateTime toDateTime(long unixTimeStamp)
        {
            return UnixEpoch.AddMilliseconds(unixTimeStamp);
        }
        public static long toUnixTime(DateTime d)
        {
            return (long)(d - UnixEpoch).TotalMilliseconds;
        }
        
        public static byte[] getHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string getHashString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in getHash(inputString))
                sb.Append(b.ToString("X2"));
            return sb.ToString().ToLower();
        }
        public static List<T> GetPage<T>(List<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }
       
        public static string StringEncrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                    ms.Dispose();
                    ms.Close();
                }
            }
            return clearText;
        }
        public static string StringDecrypt(string encryptedText)
        {
            string EncryptionKey = "abc123";
            encryptedText = encryptedText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encryptedText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    encryptedText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return encryptedText;
        }


        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }


        private static int Rand_gen(string x)
        {
            Random rnd = new Random();
            int random_num = 0;
            if (x == "even")
            {
                do
                {
                    random_num = rnd.Next(10);
                } while (random_num % 2 == 1);
            }
            else if (x == "odd")
            {
                do
                {
                    random_num = rnd.Next(10);
                } while (random_num % 2 == 0);
            }
            return random_num;
        }
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GenerateCode(int count)
        {
            string rs = string.Empty;
            string y = DateTime.Now.Year.ToString();
            string f = string.Format("{0:D4}", count);
            rs = $"{y}-{f}";
            return rs;
        }

       

        public static string ToCombinedFileName(int Id, DateTime? DateOfDocs)
        {
            return $"{Id}_{(DateOfDocs.HasValue ? toUnixTime(DateOfDocs.Value) : 0)}_{ currentTimeMillis() }";
        }
        public static string[] GetDateFromImage(string root, string p, int ProjectId)
        {
            try
            {
                var dir = Path.Combine(new string[] { root, "user_files", p, ProjectId.ToString() });
                if (Directory.Exists(dir) && Directory.GetFiles(dir).Length > 0)
                {
                    var f = GetLastImageFromDIR(dir);
                    var str = f.Split('_');
                    var d1 = str[1] != "0" ? toDateTime(long.Parse(str[1])).ToString("MM-dd-yyyy") : string.Empty;
                    var a = new string[] { d1, toFriendlyDate(toDateTime(long.Parse(str[2]))) };
                    return a;
                }
            }
            catch (Exception) { }
            return new string[] { string.Empty, string.Empty };
        }
        public static string[] GetDateFromImage(string root, string p1, string p2, int ProjectId)
        {
            try
            {
                var dir = Path.Combine(new string[] { root, "user_files", p1, p2, ProjectId.ToString() });
                if (Directory.Exists(dir) && Directory.GetFiles(dir).Length > 0)
                {
                    var f = GetLastImageFromDIR(dir);
                    var str = f.Split('_');
                    var d1 = str[1] != "0" ? toDateTime(long.Parse(str[1])).ToString("MM-dd-yyyy") : string.Empty;
                    var a = new string[] { d1, toFriendlyDate(toDateTime(long.Parse(str[2]))) };
                    return a;
                }
            }
            catch (Exception) { }
            return new string[] { string.Empty, string.Empty };
        }
        public static string[] GetDateFromImage(string root, string p1, string p2, string p3, int ProjectId)
        {
            try
            {
                var dir = Path.Combine(new string[] { root, "user_files", p1, p2, p3, ProjectId.ToString() });
                if (Directory.Exists(dir) && Directory.GetFiles(dir).Length > 0)
                {
                    var f = GetLastImageFromDIR(dir);
                    var str = f.Split('_');
                    var d1 = str[1] != "0" ? toDateTime(long.Parse(str[1])).ToString("MM-dd-yyyy") : string.Empty;
                    var a = new string[] { d1, toFriendlyDate(toDateTime(long.Parse(str[2]))) };
                    return a;
                }
            }
            catch (Exception) { }
            return new string[] { string.Empty, string.Empty };
        }
        public static string GetLastImageFromDIR(string path)
        {
            var dir = Directory.GetFiles(path).OrderByDescending(x => File.GetCreationTime(x)).ToArray();
            return dir.Length > 0 ? Path.GetFileNameWithoutExtension(dir[0]) : string.Empty;
        }

        public static double trimDecimal(double number, int zero)
        {
            return Math.Round(number, zero);
        }

        public static int GetAge(DateTime birthday)
        {
            return (DateTime.Now.Year - birthday.Year);
        }
        public static int GetAge(long birthday)
        {
            var d1 = toDateTime(birthday);
            return (DateTime.Now.Year - d1.Year);
        }

        public static void SendEmail(string emailTo, string subject, string msgBody)
        {
            string _CompanyGmail = "pesomisamisoriental@gmail.com";
            string _CompanyGmailPassword = "@PESOmisoriental";
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(_CompanyGmail, _CompanyGmailPassword);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(_CompanyGmail);
            msg.To.Add(new MailAddress(emailTo));

            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = msgBody;
            client.Send(msg);
        }
        public static void EmptyFolder(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                EmptyFolder(subfolder);
            }
        }
        public static StreamContent CreateFileContent(System.IO.Stream stream, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"files\"",
                FileName = "\"" + fileName + "\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static string GetMonthFromNumber(int n)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(n);
        }

        public static string GetQRCode()
        {
            Random r = new Random();
            var x = r.Next(0, 100000);
            string s = x.ToString("D5");
            return $"{DateTime.Now.ToString("yy")}{DateTime.Now.Month.ToString("D2")}{s}";
        }
        public static string GetQRCodeCashAdvance(string prepend)
        {
            Random r = new Random();
            var x = r.Next(0, 100000);
            string s = x.ToString("D5");
            return $"{prepend}{DateTime.Now.ToString("yy")}{DateTime.Now.Month.ToString("D2")}{s}";
        }

        public static string DisplayImageToHTML(byte[] b)
        {
            var base64 = Convert.ToBase64String(b);
            var imgSrc = String.Format("data:image/gif;base64,{0}", base64);
            return imgSrc;
        }
        public static void CopyDir(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            // Get Files & Copy
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);

                // ADD Unique File Name Check to Below!!!!
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }

            // Get dirs recursively and copy files
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyDir(folder, dest);
            }
        }
    }
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
        /// <summary>
        /// Checks to be sure a phone number contains 10 digits as per American phone numbers.  
        /// If 'IsRequired' is true, then an empty string will return False. 
        /// If 'IsRequired' is false, then an empty string will return True.
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="IsRequired"></param>
        /// <returns></returns>
        public static bool ValidatePhoneNumber(this string phone, bool IsRequired)
        {
            if (string.IsNullOrEmpty(phone) & !IsRequired)
                return true;

            if (string.IsNullOrEmpty(phone) & IsRequired)
                return false;

            var cleaned = phone.RemoveNonNumeric();
            if (IsRequired)
            {
                if (cleaned.Length == 10)
                    return true;
                else
                    return false;
            }
            else
            {
                if (cleaned.Length == 0)
                    return true;
                else if (cleaned.Length > 0 & cleaned.Length < 10)
                    return false;
                else if (cleaned.Length == 10)
                    return true;
                else
                    return false; // should never get here
            }
        }

        /// <summary>
        /// Removes all non numeric characters from a string
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string RemoveNonNumeric(this string phone)
        {
            return Regex.Replace(phone, @"[^0-9]+", "");
        }



    }

}
