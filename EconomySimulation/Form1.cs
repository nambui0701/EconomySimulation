using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;

namespace EconomySimulation
{

    public partial class Form1 : Form
    {
        public Stopwatch stopWatch = new Stopwatch();

        
        public static int maxIntensity = 10;
        public static int maxShares = maxIntensity * 1000;

        public bool tempBool = false;
        public bool tempBool1 = false;
        public bool tempBool2 = false;
        public bool tempBool3 = false;

        public bool tempBool4 = false;
        public bool tempBool5 = false;
        public bool tempBool6 = false;
        public bool tempBool7 = false;


        private Thread UpdateChart;
        private Thread Firm1Thread;
        private Thread Firm2Thread;


        private Random random = new Random(System.DateTime.Today.Day);

        public List<int> Ledger = new List<int>();

        public double bidFund1;
        public double bidFund2;

        public Form1()
        {
            stopWatch.Start();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //main update thread
            UpdateChart = new Thread(new ThreadStart(this.getNextEconPrice));
            Firm1Thread = new Thread(new ThreadStart(Fund1.mainLoop));
            Firm2Thread = new Thread(new ThreadStart(Fund2.mainLoop));
            Firm1Thread.IsBackground = true;
            Firm2Thread.IsBackground = true;
            UpdateChart.IsBackground = true;
            UpdateChart.Start();
            Firm1Thread.Start();
            Firm2Thread.Start();
        }

        private void getNextEconPrice()
        { 
            while (true)
            {
                // random price change of market
                // Simulates efficient market hypothesis which states that the market is unpredictible and is close to being totally random

                double max1 = variables.price * 0.06f;
                double min1 = variables.price * 0.04f;
                double max2 = variables.price * 0.05f;
                double min2 = variables.price * 0.03f;

                int randomInt = random.Next(1, 10);
                if (randomInt < 5)
                {
                    // generates and adds that random price to the equity
                    variables.price += random.NextDouble() * (random.NextDouble() * (max1 - min1) + min1);
                }
                else
                {
                    // generates and subtracts that random price from the equity
                    variables.price -= random.NextDouble() * (random.NextDouble() * (max2-min2) + min2);
                }

                // Models the effect of funds on the equity
                // the underlying assumption is that the funds have a higher effect on stocks than randomness does
             
                if (Fund1.returnIntent() == "buy" || Fund1.returnIntent() == "sell" || Fund2.returnIntent() == "buy" || Fund2.returnIntent() == "sell")
                {
                    if (Fund1.returnIntent() == "buy" && Fund2.returnIntent() == "buy")
                    {
                        tempBool = true;
                        variables.price += variables.price * Sigmoid(Fund1.returnIntensity()+Fund2.returnIntensity()) * 0.01f;
                    }
                    if (Fund1.returnIntent() == "buy" && Fund2.returnIntent() == "sell")
                    {
                        tempBool1 = true;
                        variables.price += variables.price * Sigmoid(Math.Abs(Fund1.returnIntensity()-Fund2.returnIntensity())) * 0.01f;
                    }
                    if (Fund1.returnIntent() == "sell" && Fund2.returnIntent() == "buy")
                    {
                        tempBool2 = true;
                        variables.price -= variables.price * Sigmoid(Math.Abs(Fund1.returnIntensity() - Fund2.returnIntensity())) * 0.01f;
                    }
                    if (Fund1.returnIntent() == "sell" && Fund2.returnIntent() == "sell")
                    {
                        tempBool3 = true;
                        variables.price -= variables.price * Sigmoid(Fund1.returnIntensity() + Fund2.returnIntensity()) * 0.01f;
                    }
                    if (Fund1.returnIntent() == "buy")
                    {
                        tempBool4 = true;
                        variables.price += variables.price * Sigmoid(Fund1.returnIntensity()) * 0.01f;
                    }
                    if (Fund1.returnIntent() == "sell")
                    {
                        tempBool5 = true;
                        variables.price -= variables.price * Sigmoid(Fund1.returnIntensity())*0.01f;
                    }
                    if (Fund2.returnIntent() == "buy")
                    {
                        tempBool6 = true;
                        variables.price += variables.price * Sigmoid(Fund2.returnIntensity()) * 0.01f;
                    }
                    if (Fund2.returnIntent() == "sell")
                    {
                        tempBool7 = true;
                        variables.price -= variables.price * Sigmoid(Fund2.returnIntensity())*0.01f;
                    }

                    Fund1.buyIntent = false;
                    Fund2.buyIntent = false;
                    Fund1.sellIntent = false;
                    Fund2.sellIntent = false;
                }
                // adds the random price to the equity array
                variables.economyArray[variables.economyArray.Length - 1] = variables.price;
                variables.Fund1MoneyArray[variables.Fund1MoneyArray.Length - 1] = Fund1.Money;
                variables.Fund2MoneyArray[variables.Fund2MoneyArray.Length - 1] = Fund2.Money;
                variables.Fund1Return[variables.Fund1Return.Length - 1] = Fund1.returnGainsLosses();
                variables.Fund2Return[variables.Fund2Return.Length - 1] = Fund2.returnGainsLosses();
                
                // copying array
                Array.Copy(variables.economyArray, 1, variables.economyArray, 0, variables.economyArray.Length - 1);
                Array.Copy(variables.Fund1MoneyArray, 1, variables.Fund1MoneyArray, 0, variables.Fund1MoneyArray.Length - 1);
                Array.Copy(variables.Fund2MoneyArray, 1, variables.Fund2MoneyArray, 0, variables.Fund2MoneyArray.Length - 1);
                Array.Copy(variables.Fund1Return, 1, variables.Fund1Return, 0, variables.Fund1Return.Length - 1);
                Array.Copy(variables.Fund2Return, 1, variables.Fund2Return, 0, variables.Fund2Return.Length - 1);

                if (chart1.IsHandleCreated)
                {
                    // makes delegatet to update chart
                    this.Invoke((MethodInvoker)delegate { UpdateEquityChart(); });
                    this.Invoke((MethodInvoker)delegate { printToTextBox(); });
                }
                else
                {
                    //do something
                }

                Thread.Sleep(100);
            }
        }
        private void UpdateEquityChart()
        {
            // clears point before entering loo
            chart1.Series["Series1"].Points.Clear();
            // a for loop to cycle through the economic array to print on screen
            for (int i = 0; i < variables.economyArray.Length-1; i++)
            {
                // adds the I element of the econ array to the chart
                chart1.Series["Series1"].Points.AddY(variables.economyArray[i]);
                TimeSpan ts = stopWatch.Elapsed;
                textBox3.Text = ts.ToString();
            }
            /*
            chart2.Series["Series1"].Points.Clear();
            for (int i = 0; i < variables.Fund1MoneyArray.Length-1 ; i++)
            {
                chart2.Series["Series1"].Points.AddY(variables.Fund1MoneyArray[i]);
            }
            chart3.Series["Series1"].Points.Clear();
            for (int i = 0; i < variables.Fund2MoneyArray.Length - 1; i++)
            {
                chart3.Series["Series1"].Points.AddY(variables.Fund2MoneyArray[i]);
            }
            */
            chart2.Series["Series2"].Points.Clear();
            for (int i = 0; i < variables.Fund1Return.Length - 1; i++)
            {
                chart2.Series["Series2"].Points.AddY(variables.Fund1Return[i]);
            }
            chart3.Series["Series2"].Points.Clear();
            for (int i = 0; i < variables.Fund2Return.Length - 1; i++)
            {
                chart2.Series["Series2"].Points.AddY(variables.Fund2Return[i]);
            }

        }

        // fund 1, bigger than fund 2;
        // makeing fund 1;
        public void printToTextBox()
        {
            richTextBox2.Text += String.Format("\nFund 1: " + Fund1.Money + "  |  Fund 2: " + Fund2.Money);

            Fund1.setShares();
            Fund2.setShares();
            if(tempBool)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Buy | Buy" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool1)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Buy | Sell" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool2)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Sell | Buy" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool3)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Sell | Sell" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool4)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Buy | Neutral" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool5)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Sell | Neutral" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool6)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Neutral | Buy" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            else if (tempBool7)
                richTextBox1.Text += "\nFunds are about to Trade : " + stopWatch.Elapsed.ToString() + " Neutral | Sell" + " " + Fund1.Shares * 1000f + " | " + Fund2.Shares * 1000f;
            tempBool = false;
            tempBool1 = false;
            tempBool2 = false;
            tempBool3 = false;
            tempBool4 = false;
            tempBool5 = false;
            tempBool6 = false;
            tempBool7 = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateChart.Suspend();
                Firm1Thread.Suspend();
                Firm2Thread.Suspend();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        public double Sigmoid(double x)
        {
            return (4.9f/((0.5f + Math.Exp((-4.7f * x)+3.8f))));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }
    }

    public static class Fund1
    {
        public static double Money = 500;
        public static double Shares = 0;
        public static double Intensity = 0;
        public static bool buyIntent;
        public static bool sellIntent;
        public static int investmentPower;
        static Fund1()
        {
            // the power a fund has to invest
            // calculated out of 100
            investmentPower = 100;
            // Whether the fund has intent to buy or not.
            buyIntent = false;
            // Whether the fund has intent to sell or not.
            sellIntent = false;
            // How much money the fund has.
        }
        public static double SigmoidBuy(double x)
        {
            return (0.8f / (1 + Math.Exp((-x + 5.8f))));
        }
        // function to set the number of share to the intensity which is the number of shares and multiplies by factor of 100
        public static void setShares()
        {
            Shares = (Intensity * 1000f);
        }
        public static double returnGainsLosses()
        {
            return ((Shares * variables.price) + Money) / 500;
        }
        // function to return a number of shares, cannot be called unless setShares() is called first
        public static double returnShares()
        {
            return Shares;
        }
        // function to return the intensity, or the raw value on the number of shares the fund wants to sell
        public static double returnIntensity()
        {
            return Intensity;
        }
        // a function to return the intent (Whether the fund wants to buy or sell or is neutral)
        public static string returnIntent()
        {
            if (buyIntent)
                return "buy";
            if (sellIntent)
                return "sell";
            else
                return "neutral";
        }

        // A function that sets the intent of the fund to a boolean value
        public static void WantToSell(double shares)
        {
            if (Shares != 0)
            {
                sellIntent = true;
                buyIntent = false;
                Intensity = shares;
                setShares();
                Money += Shares * variables.price;
            }

        }

        // A function that sets the intent of the fund to a boolean value
        public static void WantToBuy(double shares)
        {
            // checks whether the fund can buy the stock or not
            if ((Shares * variables.price) <= Money)
            {
                buyIntent = true;
                sellIntent = false;
                Intensity = shares;
                setShares();
                Money -= Shares * variables.price;

            }
        }

        // The main loop that rund asyncrounously with both the random function as well as the graphing and information display
        // The buy function of this fund is modeled by a Cosine function, the input is the moving is the percent difference between the 50 day moving average and the current price
        public static void mainLoop()
        {
            Random random = new Random();
            List<double> priceList = new List<double>();
            int iterator = 0;
            while (true)
            {
                priceList.Add(variables.price);
                if (iterator > 50)
                {
                    /*
                    double movingAverage = 0;
                    int px;
                    for (px = priceList.Count - 50; px < priceList.Count-10; px++)
                    {
                        movingAverage += priceList[px];
                    }
                    movingAverage = movingAverage/px;
                    */

                    double movingAverage = priceList[priceList.Count - 2];
                    double currentPrice = priceList[priceList.Count - 1];
                    // if the current price is less than the price from 10 to 30 iteration before...
                    if (currentPrice < movingAverage)
                    {
                        WantToSell((double)SigmoidBuy(((currentPrice - movingAverage) / currentPrice)));

                    }
                    // if the current price is more than the price 10 to 30 iterations before iteration before...
                    if (currentPrice > movingAverage)
                    {

                        WantToBuy((double)SigmoidBuy(((movingAverage - currentPrice) / movingAverage)));
                    }

                 
                }
                Thread.Sleep(200);
                iterator++;
            }
        }
    }

    public static class Fund2
    {
        public static double Money = 500;
        public static double Shares = 0;
        public static double Intensity = 0;
        public static bool buyIntent;
        public static bool sellIntent;
        public static int investmentPower;
        static Fund2()
        {
            // the power a fund has to invest
            // calculated out of 100
            investmentPower = 100;
            // Whether the fund has intent to buy or not.
            buyIntent = false;
            // Whether the fund has intent to sell or not.
            sellIntent = false;
            // How much money the fund has.
        }
        public static double SigmoidBuy(double x)
        {
            return (0.8f / (1 + Math.Exp((-x + 6.6f))));
        }
        // function to set the number of share to the intensity which is the number of shares and multiplies by factor of 100
        public static void setShares()
        {
            Shares = (Intensity * 1000f);
        }
        // function to return a number of shares, cannot be called unless setShares() is called first
        public static double returnShares()
        {
            return Shares;
        }
        public static double returnGainsLosses()
        {
            return ((Shares * variables.price) + Money) / 500;
        }
        // function to return the intensity, or the raw value on the number of shares the fund wants to sell
        public static double returnIntensity()
        {
            return Intensity;
        }
        // a function to return the intent (Whether the fund wants to buy or sell or is neutral)
        public static string returnIntent()
        {
            if (buyIntent)
                return "buy";
            if (sellIntent)
                return "sell";
            else
                return "neutral";
        }

        // A function that sets the intent of the fund to a boolean value
        public static void WantToSell(double shares)
        {
            if (Shares != 0)
            {
                sellIntent = true;
                buyIntent = false;
                Intensity = shares;
                setShares();
                Money += Shares * variables.price;
            }

        }

        // A function that sets the intent of the fund to a boolean value
        public static void WantToBuy(double shares)
        {
            // checks whether the fund can buy the stock or not
            if ((Shares * variables.price) <= Money)
            {
                buyIntent = true;
                sellIntent = false;
                Intensity = shares;
                setShares();
                Money -= Shares * variables.price;

            }
        }

        // The main loop that rund asyncrounously with both the random function as well as the graphing and information display
        // The buy function of this fund is modeled by a Cosine function, the input is the moving is the percent difference between the 50 day moving average and the current price
        public static void mainLoop()
        {
            Random random = new Random();
            List<double> priceList = new List<double>();
            int iterator = 0;
            while (true)
            {
                priceList.Add(variables.price);
                if (iterator > 50)
                {
                    double movingAverage;
                    movingAverage = priceList[priceList.Count - 2];

                    double currentPrice = priceList[priceList.Count - 1];
                    // if the current price is less than the price from 10 to 30 iteration before...
                    if (currentPrice < movingAverage)
                    {
                        WantToSell((double)SigmoidBuy((currentPrice - movingAverage) / currentPrice));

                    }
                    // if the current price is more than the price 10 to 30 iterations before iteration before...
                    if (currentPrice > movingAverage)
                    {

                        WantToBuy((double)SigmoidBuy((movingAverage - currentPrice) / movingAverage));
                    }


                }
                Thread.Sleep(200);
                iterator++;
            }
        }
    }
    public static class variables
    {
        public static double price = 50;
        public static double[] economyArray = new double[100];
        public static double[] Fund1MoneyArray = new double[100];
        public static double[] Fund2MoneyArray = new double[100];
        public static double[] Fund1Return = new double[100];
        public static double[] Fund2Return = new double[100];
    }
}
