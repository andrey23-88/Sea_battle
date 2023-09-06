using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Battleship_college
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int BoardSize = 10;//размер одной палубы корабля
        private Button[,] playerBoard;//наше поле
        private Button[,] computerBoard;//поле компьютера
        private bool isPlayerTurn;//проверка очередности хода
        private bool isGameOver;//проверка на проигрыш
        private int playerShipRemaining = 0;//количество оставшихся кораблей игрока
        private int computerShipRemaining = 0;//количество оставшихся кораблей противника
        List<Ship> ship = new List<Ship>();
        List<Ship> shipComp = new List<Ship>();
        private Storyboard storyboard;//для анимации кнопки
        

        public MainWindow()
        {
            InitializeComponent();
            CreateBoards();//создание кораблей
            isPlayerTurn = true;//первый ходит игрок
            isGameOver = false;            
            AnimateButtonBackground(startBtn);           
        }      

        private void AnimateButtonBackground(Button button)
        {
            // Создаем анимацию изменения цвета фона кнопки
            ColorAnimation animation = new ColorAnimation();
            animation.To = Colors.Blue;
            animation.Duration = TimeSpan.FromSeconds(3);
            animation.AutoReverse = true;
            
            // Создаем и запускаем storyboard
            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));
            Storyboard.SetTarget(animation, button);
            // Задаем бесконечное повторение анимации
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Begin();            
        }

        private void StopButtonAnimation(Button button)
        {            
            storyboard.Stop();
        }


        private void CreateBoards()
        {
            playerBoard = new Button[BoardSize, BoardSize];
            computerBoard = new Button[BoardSize, BoardSize];

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    //создаем кнопки  
                    Button playerButton = new Button();
                    Button computerButton = new Button();
                    computerButton.Click += PlayerBoard_Click;                    

                    playerButton.IsEnabled = false;
                    computerButton.IsEnabled = false;

                    //указываем размер игрового поля
                    playerGrid.Children.Add(playerButton);
                    Grid.SetColumn(playerButton, col);
                    Grid.SetRow(playerButton, row);

                    computerGrid.Children.Add(computerButton);
                    Grid.SetColumn(computerButton, col);
                    Grid.SetRow(computerButton, row);

                    playerBoard[row, col] = playerButton;
                    computerBoard[row, col] = computerButton;
                }
            }
        }  


        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }
        private void ResetGame()
        {
            StopButtonAnimation(startBtn);
            AnimateButtonBackground(restBtn);
            startBtn.IsEnabled = false;
            restBtn.IsEnabled = true;
            //делает активными кнопки на полях
            foreach (var child in playerGrid.Children)
            {
                if (child is Button)
                {
                    ((Button)child).IsEnabled = true;
                    ((Button)child).Background = Brushes.White;
                }
            }
            foreach (var child in computerGrid.Children)
            {
                if (child is Button)
                {
                    ((Button)child).IsEnabled = true;
                    ((Button)child).Background = Brushes.White;
                }
            }

            isPlayerTurn = true;
            isGameOver = false;
            playerShipRemaining = 10;//оставшиеся корабли игрока
            computerShipRemaining = 10;//оставшиеся корабли компьютера

            plship.Text = playerShipRemaining.ToString();
            compsh.Text = computerShipRemaining.ToString();

            //очищаем наше поле
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    playerBoard[row, col].IsEnabled = true;
                    playerBoard[row, col].Content = "";
                }
            }
            //очищаем поле компьютера
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    computerBoard[row, col].IsEnabled = true;
                    computerBoard[row, col].Content = "";
                }
            }
            //очистка коллекций кораблей
            ship.Clear();
            shipComp.Clear();
            
            //расстановка кораблей на поле
            PlaceShips1(true,playerBoard);
            PlaceShips1(false, computerBoard);
            
            computerShipRemaining = 10;
            playerShipRemaining = 10;
        }
        private void restBtn_Click(object sender, RoutedEventArgs e)
        {
            restBtn.Content = "Сдасться";
            StopButtonAnimation(restBtn);
            AnimateButtonBackground(startBtn);
            startBtn.IsEnabled = true;
            restBtn.IsEnabled = false;
            int x, y;
            ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/k5.png", UriKind.Relative)));
            //показ расстановки кораблей противника
            foreach (var item in shipComp)
            {
                if (item.x1 == "") continue;
                x = int.Parse(item.x1); y = int.Parse(item.y1);
                computerBoard[x, y].Background = brush;
                if (item.x2 == "") continue;
                x = int.Parse(item.x2); y = int.Parse(item.y2);
                computerBoard[x, y].Background = brush;
                if (item.x3 == "") continue;
                x = int.Parse(item.x3); y = int.Parse(item.y3);
                computerBoard[x, y].Background = brush;
                if (item.x4 == "") continue;
                x = int.Parse(item.x4); y = int.Parse(item.y4);
                computerBoard[x, y].Background = brush;
            }
        }

        private void PlayerBoard_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn || isGameOver) { ComputerMove(); return; }

            //определяем нажатую кнопку, получаем значения строки и столбца из нее
            var button = sender as Button;
            int col = Grid.GetColumn(button);
            int row = Grid.GetRow(button);
            //проверяем был ли выстрел по этой ячейке
            if (computerBoard[row, col].Content.ToString() != "" &&
                computerBoard[row, col].Content.ToString() != "◘")
            {
                return;
            }
            //если попал, ставим Х, уменьшаем кол-во оставшихся кораблей
            if (computerBoard[row, col].Content.ToString() == "◘")
            {
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/poom.png", UriKind.Relative)));
                
                computerBoard[row, col].Background = brush; //
                computerBoard[row, col].Content = "X";

                //прорисовка подбитого корабля на поле
                foreach (var item in shipComp)
                {
                    if (item.x1 != "")
                        if (row == int.Parse(item.x1) && col == int.Parse(item.y1))
                        { item.x1 = ""; item.y1 = ""; item.pal--; break; }
                    if (item.x2 != "")
                        if (row == int.Parse(item.x2) && col == int.Parse(item.y2))
                        { item.x2 = ""; item.y2 = ""; item.pal--; break; }
                    if (item.x3 != "")
                        if (row == int.Parse(item.x3) && col == int.Parse(item.y3))
                        { item.x3 = ""; item.y3 = ""; item.pal--; break; }
                    if (item.x4 != "")
                        if (row == int.Parse(item.x4) && col == int.Parse(item.y4))
                        { item.x4 = ""; item.y4 = ""; item.pal--; break; }
                }
                computerShipRemaining = 0;
                //подсчет оставшихся кораблей
                foreach (var item in shipComp)
                {
                    if (item.pal != 0) computerShipRemaining++;
                }
                plship.Text = playerShipRemaining.ToString();
                compsh.Text = computerShipRemaining.ToString();
                //анимация  цифры количества кораблей
                CreatePulseAnimation(compsh, 5, 500);
            }
            else
            {
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/boom.png", UriKind.Relative)));
                //прорисовка промаха на поле
                computerBoard[row, col].Background = brush;
                computerBoard[row, col].Content = "-";
                isPlayerTurn = false;
            }
            computerShipRemaining = 0;
            //подсчет оставшихся кораблей
            foreach (var item in shipComp)
            {
                if (item.pal != 0) computerShipRemaining++;
            }
                       
            //проверяем условие победы
            if (computerShipRemaining == 0)
            {
                isGameOver = true;
                restBtn.Content = "Переиграть";
                MessageBox.Show("Вы победили!");
            }
            else
            {
                ComputerMove();//ход компьютера
            }
        }
        //ход компьютера
        private void ComputerMove()
        {
            if (isPlayerTurn || isGameOver) return;

            Random random = new Random();
            int row, col;
            bool a, b;
            //подбор пустой ячейки для стрельбы

            while (true)
            {
                row = random.Next(0, BoardSize);
                col = random.Next(0, BoardSize);
                a = playerBoard[row, col].Content.ToString() =="◘";
                b = playerBoard[row, col].Content.ToString() == "";
                 
                if (a) break;
                else if(b) break;
            } 
            //обработка выстрела компьютера
            if (playerBoard[row, col].Content.ToString() == "◘")
            {
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/poom.png", UriKind.Relative)));
                playerBoard[row, col].Background = brush;
                playerBoard[row, col].Content = "X";
                plship.Text = playerShipRemaining.ToString();
                
                
                //анимация цифры на попадание противником
                CreatePulseAnimation(plship, 3, 500);
                //прорисовка подбитого корабля на поле
                foreach (var item in ship)
                {
                    if(item.x1 != "") 
                        if(row == int.Parse(item.x1) && col == int.Parse(item.y1))
                        { item.x1 = ""; item.y1 = ""; item.pal--; break;}
                    if(item.x2 != "") 
                        if(row == int.Parse(item.x2) && col == int.Parse(item.y2))
                        { item.x2 = ""; item.y2 = ""; item.pal--; break; }
                    if(item.x3 != "") 
                        if(row == int.Parse(item.x3) && col == int.Parse(item.y3))
                        { item.x3 = ""; item.y3 = ""; item.pal--; break; }
                    if (item.x4 != "") 
                        if(row == int.Parse(item.x4) && col == int.Parse(item.y4))
                        { item.x4 = ""; item.y4 = ""; item.pal--; break; }
                }
            }
            else
            {
                //прорисовка промаха на поле
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/boom.png", UriKind.Relative)));
                playerBoard[row, col].Background = brush;
                playerBoard[row, col].Content = "-";
                isPlayerTurn = true;
            }
            //подсчет оставшихся кораблей
            playerShipRemaining = 0;
            foreach (var item in ship)
            {
                if (item.pal != 0) playerShipRemaining++;
            }
            //пишем результаты в окно
            plship.Text = playerShipRemaining.ToString();
            compsh.Text = computerShipRemaining.ToString();
            
            //проверяем условие победы
            if (playerShipRemaining == 0)
            {
                isGameOver = true;
                restBtn.Content = "Переиграть";
                MessageBox.Show("Компьютер победил!");
            }
        }
        //расстановка кораблей
        private void PlaceShips1(bool temp, Button[,] board)
        {
            List<int> koord = new List<int>();
            Random random = new Random();
            int x, y = 0;

            while (true)
            {
                x = random.Next(7);
                y = random.Next(7);
                bool isHorizontal = random.Next(2) == 0;
                //расстановка 4-х палубного
                if (isHorizontal)//горизонтально
                {
                    if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                    else { koord.Clear(); continue; }
                    if (Test(x, y + 1, board)) { koord.Add(x); koord.Add(y + 1); }
                    else { koord.Clear(); continue; }
                    if (Test(x, y + 2, board)) { koord.Add(x); koord.Add(y + 2); }
                    else { koord.Clear(); continue; }
                    if (Test(x, y + 3, board)) { koord.Add(x); koord.Add(y + 3); }
                    else { koord.Clear(); continue; }
                    break;
                }
                else//вертикально
                {
                    if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                    else { koord.Clear(); continue; }
                    if (Test(x + 1, y, board)) { koord.Add(x + 1); koord.Add(y); }
                    else { koord.Clear(); continue; }
                    if (Test(x + 2, y, board)) { koord.Add(x + 2); koord.Add(y); }
                    else { koord.Clear(); continue; }
                    if (Test(x + 3, y, board)) { koord.Add(x + 3); koord.Add(y); }
                    else { koord.Clear(); continue; }
                    break;
                }
            }

            if (temp)//для игрока
            {
                ship.Add(new Ship(4, koord[0].ToString(), koord[1].ToString(),
                koord[2].ToString(), koord[3].ToString(), koord[4].ToString(),
                koord[5].ToString(),
                koord[6].ToString(), koord[7].ToString()));
                koord.Clear();
                MyPlaceShips(board);
            }
            else//для компьютера
            {
                shipComp.Add(new Ship(4, koord[0].ToString(), koord[1].ToString(),
                koord[2].ToString(), koord[3].ToString(), koord[4].ToString(),
                koord[5].ToString(),
                koord[6].ToString(), koord[7].ToString()));
                koord.Clear();
                CompPlaceShips(board);
            }

            for (int i = 0; i < 2; i++)//расстановка 3-х палубного
            {
                while (true)
                {
                    x = random.Next(8);
                    y = random.Next(8);
                    bool isHorizontal = random.Next(2) == 0;
                    if (isHorizontal)//горизонтально
                    {
                        if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        if (Test(x, y + 1, board)) { koord.Add(x); koord.Add(y + 1); }
                        else { koord.Clear(); continue; }
                        if (Test(x, y + 2, board)) { koord.Add(x); koord.Add(y + 2); }
                        else { koord.Clear(); continue; }
                        break;
                    }
                    else//вертикально
                    {
                        if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        if (Test(x + 1, y, board)) { koord.Add(x + 1); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        if (Test(x + 2, y, board)) { koord.Add(x + 2); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        break;
                    }
                }
                if (temp)//для игрока
                {
                    ship.Add(new Ship(3, koord[0].ToString(), koord[1].ToString(),
                    koord[2].ToString(), koord[3].ToString(), koord[4].ToString(),
                    koord[5].ToString()));
                    koord.Clear();
                    MyPlaceShips(board);
                }
                else//для компьютера
                {
                    shipComp.Add(new Ship(3, koord[0].ToString(), koord[1].ToString(),
                    koord[2].ToString(), koord[3].ToString(), koord[4].ToString(),
                    koord[5].ToString()));
                    koord.Clear();                    
                    CompPlaceShips(board);
                }
            }

            for (int i = 0; i < 3; i++)//расстановка 2-х палубного
            {
                while (true)
                {
                    x = random.Next(9);
                    y = random.Next(9);
                    bool isHorizontal = random.Next(2) == 0;
                    if (isHorizontal)//горизонтально
                    {
                        if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        if (Test(x, y + 1, board)) { koord.Add(x); koord.Add(y + 1); }
                        else { koord.Clear(); continue; }
                        break;
                    }
                    else//вертикально
                    {
                        if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        if (Test(x + 1, y, board)) { koord.Add(x + 1); koord.Add(y); }
                        else { koord.Clear(); continue; }
                        break;
                    }
                }
                if (temp)//для игрока
                {
                    ship.Add(new Ship(2, koord[0].ToString(), koord[1].ToString(),
                    koord[2].ToString(), koord[3].ToString()));
                    koord.Clear();
                    MyPlaceShips(board);
                }
                else//для компьютера
                {
                    shipComp.Add(new Ship(2, koord[0].ToString(), koord[1].ToString(),
                    koord[2].ToString(), koord[3].ToString()));
                    koord.Clear();
                    CompPlaceShips(board);
                }
            }

            for (int i = 0; i < 4; i++)//расстановка 1-х палубного
            {
                while (true)
                {
                    x = random.Next(10);
                    y = random.Next(10);
                    if (Test(x, y, board)) { koord.Add(x); koord.Add(y); }
                    else { koord.Clear(); continue; } 
                    break;
                }
               
               if(temp)//для игрока
                {
                    ship.Add(new Ship(1, koord[0].ToString(), koord[1].ToString()));
                    koord.Clear(); 
                    MyPlaceShips(board);
                }
                else//для компьютера
                {
                    shipComp.Add(new Ship(1, koord[0].ToString(), koord[1].ToString()));
                    koord.Clear();
                    CompPlaceShips(board);
                }               
            }
        }
        //проверка на возможность расстановки
        private bool Test(int x, int y, Button[,] board)
        {
            if(board[x, y].Content.ToString() != "") return false;
            if ((x>0 && x<9) && (y > 0 && y < 9)) { return Full(x,y,board); }
            else if (x == 0 && y == 0) { return XYZero(x, y, board); }
            else if (x == 9 && y == 9) { return XYNine(x, y, board); }
            else if (x == 9 && y == 0) { return XNineYZero(x, y, board); }
            else if (x == 0 && y == 9) { return XZeroYNine(x, y, board); }
            else if (x == 0 ) { return XZero(x, y, board); }
            else if (x == 9) { return XNine(x, y, board); }
            else if (y == 0) { return YZero(x, y, board); }
            else if (y == 9) { return YNine(x, y, board); }
            return false; 
        }
        //проверка на возможность расстановки при  у=9
        private bool YNine(int x, int y, Button[,] board)
        {
            if (board[x, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y].Content.ToString() != "") return false;
            if (board[x + 1, y].Content.ToString() != "") return false;
            if (board[x + 1, y - 1].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при  у=0
        private bool YZero(int x, int y, Button[,] board)
        {
            
            if (board[x - 1, y].Content.ToString() != "") return false;
            if (board[x - 1, y + 1].Content.ToString() != "") return false;
            if (board[x, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при х=9
        private bool XNine(int x, int y, Button[,] board)
        {
            if (board[x, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y].Content.ToString() != "") return false;
            if (board[x - 1, y + 1].Content.ToString() != "") return false;
            if (board[x, y + 1].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при х=0
        private bool XZero(int x, int y, Button[,] board)
        {
            if (board[x, y - 1].Content.ToString() != "") return false;
            if (board[x, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y].Content.ToString() != "") return false;
            if (board[x + 1, y - 1].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при прочих значениях
        private bool Full(int x, int y, Button[,] board)
        {
            if (board[x,     y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y    ].Content.ToString() != "") return false;
            if (board[x - 1, y + 1].Content.ToString() != "") return false;
            if (board[x,     y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y    ].Content.ToString() != "") return false;
            if (board[x + 1, y - 1].Content.ToString() != "") return false;            
            return true;
        }
        //проверка на возможность расстановки при х=0 у=0
        private bool XYZero(int x, int y, Button[,] board)
        {
            if (board[x, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y + 1].Content.ToString() != "") return false;
            if (board[x + 1, y].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при х=9 у=9
        private bool XYNine(int x, int y, Button[,] board)
        {
            if (board[x, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y - 1].Content.ToString() != "") return false;
            if (board[x - 1, y].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при х=9 у=0
        private bool XNineYZero(int x, int y, Button[,] board)
        {
            if (board[x -1, y + 1].Content.ToString() != "") return false;
            if (board[x, y+1].Content.ToString() != "") return false;
            if (board[x - 1, y].Content.ToString() != "") return false;
            return true;
        }
        //проверка на возможность расстановки при х=0 у=9
        private bool XZeroYNine(int x, int y, Button[,] board)
        {
            if (board[x, y -1].Content.ToString() != "") return false;
            if (board[x + 1, y - 1].Content.ToString() != "") return false;
            if (board[x +1, y].Content.ToString() != "") return false;
            return true;
        }

        //отрисовка кораблей игрока по высчитанным координатам
        private void MyPlaceShips(Button[,] board)
        {
            int x, y;
            ImageBrush brush = new ImageBrush(new BitmapImage(new Uri("Resources/k5.png", UriKind.Relative)));
            foreach (var item in ship)
            {
                x = int.Parse(item.x1); y = int.Parse(item.y1); board[x,y].Content = "◘";
                board[x, y].Background = brush;
                if (item.x2 == "") continue;
                x = int.Parse(item.x2); y = int.Parse(item.y2); board[x, y].Content = "◘";
                board[x, y].Background = brush;
                if (item.x3 == "") continue;
                x = int.Parse(item.x3); y = int.Parse(item.y3); board[x, y].Content = "◘";
                board[x, y].Background = brush;
                if (item.x4 == "") continue;
                x = int.Parse(item.x4); y = int.Parse(item.y4); board[x, y].Content = "◘";
                board[x, y].Background = brush;
            }   
        }

        //анимация пульсирующего текста
        private void CreatePulseAnimation(TextBlock textBlock, int pulseCount, int duration)
        {
            Storyboard storyboard = new Storyboard();

            for (int i = 0; i < pulseCount; i++)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = textBlock.FontSize,
                    To = textBlock.FontSize + 5,
                    Duration = new Duration(TimeSpan.FromMilliseconds(duration / 2)),
                    AutoReverse = true
                };

                Storyboard.SetTarget(animation, textBlock);
                Storyboard.SetTargetProperty(animation, new PropertyPath(TextBlock.FontSizeProperty));

                storyboard.Children.Add(animation);
            }

            storyboard.Begin();
        }

        //расстановка кораблей компьютера по высчитанным координатам
        private void CompPlaceShips(Button[,] board)
        {
            int x, y;
            foreach (var item in shipComp)
            {
                x = int.Parse(item.x1); y = int.Parse(item.y1); board[x, y].Content = "◘";
                if (item.x2 == "") continue;
                x = int.Parse(item.x2); y = int.Parse(item.y2); board[x, y].Content = "◘";
                if (item.x3 == "") continue;
                x = int.Parse(item.x3); y = int.Parse(item.y3); board[x, y].Content = "◘";
                if (item.x4 == "") continue;
                x = int.Parse(item.x4); y = int.Parse(item.y4); board[x, y].Content = "◘";
                
            }
        }        
    }
    //класс корабли
    class Ship
        {            
            public string x1 { get; set; }
            public string x2 { get; set; }
            public string x3 { get; set; }
            public string x4 { get; set; }
            public string y1 { get; set; }
            public string y2 { get; set; }
            public string y3 { get; set; }
            public string y4 { get; set; }
            public int pal { get; set; }
           

            public Ship(int pal,
                        string x1,
                        string y1,
                        string x2 = "",
                        string y2 = "",
                        string x3 = "",
                        string y3 = "",
                        string x4 = "",
                        string y4 = "") 
            {                
                    this.x1 = x1;
                    this.x2 = x2;
                    this.x3 = x3;
                    this.x4 = x4;
                    this.y1 = y1;
                    this.y2 = y2;
                    this.y3 = y3;
                    this.y4 = y4;                    
                    this.pal = pal;                               
            }  
    }   
}
