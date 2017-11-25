﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace XYBYReader
{
    public partial class ReadChapterFrm : Form
    {
        HtmlAgilityPack.HtmlDocument document;
        string chapterAddress = "https:";
        List<BookChapterClass> bookChapterList = null;
        public ReadChapterFrm()
        {
            InitializeComponent();
        }

        private void ReadChapterFrm_Load(object sender, EventArgs e)
        {
            // string firstChapter = "https://www.readnovel.com/chapter/6969133904551803/18709155986044689";
            string firstChapter = "https://www.readnovel.com/book/7635085504787403#Catalog";
            LoadWebBook(firstChapter);

        }
        /// <summary>
        /// 加载网页形式的书
        /// </summary>
        /// <param name="address"></param>
        private void LoadWebBook(string address)
        {
            WebRequest request = null;
            WebResponse response = null;
            StreamReader sreader = null;
            WebHeaderCollection headerCollection = null;
            string datetime = string.Empty;
            try
            {
                request = WebRequest.Create(address);
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultCredentials;
                WebProxy myProxy = new WebProxy();
                request.Proxy = myProxy;
                response = (WebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                sreader = new StreamReader(stream, Encoding.UTF8);
                string strReader = sreader.ReadToEnd();
                richTextBox1.Text = strReader;

            }
            catch (Exception ex) { }
            finally
            {
                if (request != null)
                { request.Abort(); request = null; }
                if (response != null)
                { response.Close(); response = null; }
                if (headerCollection != null)
                { headerCollection.Clear(); headerCollection = null; }
                if (sreader != null)
                {
                    sreader.Close();
                    sreader = null;
                }
            }
        }
        /// <summary>
        /// 加载正文
        /// </summary>
        private void LoadBook()
        {
            document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(richTextBox1.Text);
            var name = document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[1]/h3[1]");
            richTextBox1.Clear();
            richTextBox1.Text = name.InnerHtml;
            var res = document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[2]");
            string chapter = res.InnerHtml.ToString().Replace("<p>", "\n");


            richTextBox1.AppendText(chapter);
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            FindCatalog("https://www.readnovel.com/book/7635085504787403#Catalog");
            JumpChapter(5);

        }

        private void btnNextChapter_Click(object sender, EventArgs e)
        {
            FindNextChapter();
            LoadWebBook(chapterAddress);
            LoadBook(); 
            richTextBox1.Select(0, 0);
        }
        /// <summary>
        /// 查找下一章节
        /// </summary>
        private void FindNextChapter()
        {
            if (document != null)
            {
                string nextChapter = document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[4]/div[1]/div[1]")
                    .Attributes["data-nurl"].Value.ToString();
                chapterAddress += nextChapter;

            }
        }
        /// <summary>
        /// 找出书目录
        /// </summary>
        /// <param name="address"></param>
        private void FindCatalog(string address)
        {
            if (document == null)
            {
                document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(richTextBox1.Text);

                bookChapterList = new List<BookChapterClass>();
                BookChapterClass bookChapter = null;
                for (int i = 1; i <= 50; i++)
                {
                    bookChapter = new BookChapterClass();
                    bookChapter.ChapterId = Convert.ToInt16(document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[3]/div[3]/div[2]/div[1]/ul/li[" + i + "]")
        .Attributes["data-rid"].Value.ToString());
                    bookChapter.ChapterAddress = document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[3]/div[3]/div[2]/div[1]/ul/li[" + i + "]/a")
        .Attributes["href"].Value.ToString();
                    bookChapter.ChapterName = document.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[1]/div[3]/div[3]/div[2]/div[1]/ul/li[" + i + "]/a").InnerHtml;

                    bookChapterList.Add(bookChapter);
                }

            }
        }
        /// <summary>
        /// 跳转指定章节
        /// </summary>
        /// <param name="chapterId"></param>
        private void JumpChapter(int chapterId)
        {
            chapterAddress += bookChapterList.Find(x => x.ChapterId == chapterId).ChapterAddress.ToString();

            LoadWebBook(chapterAddress);
            LoadBook();
        }
    }
}