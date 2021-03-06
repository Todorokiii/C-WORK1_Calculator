﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlerWin {
 public  class SimpleCrawler {
        //private Hashtable urls = new Hashtable();
       
    public Hashtable Urls { get; set; }
   
    public  int  Count { get; set; }
        public string StartUrl { get; set; }
        public event InformEventHandler Inform;
      
    /*static void Main(string[] args) {
      SimpleCrawler myCrawler = new SimpleCrawler();
      string startUrl = "http://www.cnblogs.com/dstang2000/";
      if (args.Length >= 1) startUrl = args[0];
      myCrawler.urls.Add(startUrl, false);//加入初始页面
      new Thread(myCrawler.Crawl).Start();
    }*/

            public   SimpleCrawler()
        {
            Urls  = new Hashtable();
            Count = 0;
        }
    public void Start() 
  {
            try
            {
                Urls.Add(StartUrl, false);
            }
            catch(ArgumentException e)
            {
               Inform(this, new InformEventArgs( ) { Url = StartUrl, Message = "已存在" });
            }
            Inform(this, new InformEventArgs() { Url = null, Message = "开始爬取" });
            while (true) {
        string current = null;
        foreach (string url in Urls.Keys) {
          if ((bool)Urls[url]) continue;
          current = url;
        }

        if (current == null || Count > 10) break;
        string html = DownLoad(current); // 下载
        Urls[current] = true;
        Count++;
        Parse(html,current);//解析,并加入新的链接
      }

            Inform(this, new InformEventArgs() { Url = null, Message = "结束爬取" });
        }

    public string DownLoad(string url) {
      try {
        WebClient webClient = new WebClient();
        webClient.Encoding = Encoding.UTF8;
        string html = webClient.DownloadString(url);
        string fileName = Count.ToString();
        File.WriteAllText(fileName, html, Encoding.UTF8);
                Inform(this, new InformEventArgs() { Url = url, Message = "成功" });
                return html;
      }
      catch (Exception ex) {
                // Console.WriteLine(ex.Message);
                Inform(this, new InformEventArgs() { Url = url, Message = ex.Message });
                return "";
      }
    }

    private void Parse(string html,string currentUrl) {
      string strRef = @"(href|HREF)[]*=[]*[""'][^""'#>]+(.html |.HTML)[""']";
      MatchCollection matches = new Regex(strRef).Matches(html);
      foreach (Match match in matches) {
        strRef = match.Value.Substring(match.Value.IndexOf('=') + 1)
                  .Trim('"', '\"', '#', '>');
        if (strRef.Length == 0) continue;

                Uri uri = new Uri(currentUrl);
                string domain = uri.Scheme + ": //" + uri.Host;

                if (Regex.IsMatch(strRef,"^[/]"))
                {
                    strRef = domain + strRef;
                }
                else if(Regex.IsMatch(strRef, "^(http|HTTP)") == false){
                    strRef = currentUrl + "/" + strRef;
                }

                if (Regex.IsMatch(strRef, domain) == false)
                    continue;
        if (Urls[strRef] == null)
                    Urls[strRef] = false;
      }
    }
  }
    public delegate void InformEventHandler(object o, InformEventArgs e);
    public class InformEventArgs : EventArgs
    {
        public string Url { get; set; }
        public string Message { get; set; }
    }
}
