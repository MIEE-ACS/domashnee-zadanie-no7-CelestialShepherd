using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        // звезда
        Star star;
        //количество очков
        int score;
        //таймер по которому осуществляется движение всех объектов
        DispatcherTimer moveTimer;
        //таймер по которому идёт действие звезды
        DispatcherTimer starActiveTimer;
        //таймер по которому идёт перезарядка звезды
        DispatcherTimer starCooldownTimer;
        //таймер по которому происходит исчезновение звезды
        DispatcherTimer starDisappearTimer;
        //множитель очков
        int multiplier = 1;
        //рандом для "честных" случайных чисел
        static Random rand = new Random();

        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);

            //создаем таймер, отсчитывающий время действия звезды(срабатывающий раз в 30 cек.)
            starActiveTimer = new DispatcherTimer();
            starActiveTimer.Interval = new TimeSpan(0, 0, 0, 30, 0);
            starActiveTimer.Tick += new EventHandler(starActiveTimer_Tick);

            //создаем таймер, отсчитывающий время перезарядки звезды(срабатывающий раз в 20 cек.)
            starCooldownTimer = new DispatcherTimer();
            starCooldownTimer.Interval = new TimeSpan(0, 0, 0, 20, 0);
            starCooldownTimer.Tick += new EventHandler(starCooldownTimer_Tick);

            //создаем таймер, отсчитывающий время после которого звезда исчезает(срабатывающий раз в 5 cек.)
            starDisappearTimer = new DispatcherTimer();
            starDisappearTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
            starDisappearTimer.Tick += new EventHandler(starDisappearTimer_Tick);
        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    starActiveTimer.Stop();
                    starCooldownTimer.Stop();
                    starDisappearTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                moveTimer.Stop();
                starActiveTimer.Stop();
                starCooldownTimer.Stop();
                starDisappearTimer.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                return;
            }

            if (head.x == star.x && head.y == star.y)
            {
                //Убираем картинку
                canvas1.Children.Remove(star.image);
                //Перемещаем звезду за пределы игрового поля
                star.x = 0;
                star.y = 0;
                //Делаем текст с действием бонуса видимым
                lblBonus.Visibility = Visibility.Visible;
                //Задаём множитель очков
                multiplier = 10;
                //Останавливаем
                starDisappearTimer.Stop();
                starActiveTimer.Start();
            }

            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                score = score + 1 * multiplier;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);
            }
            //перерисовываем экран
            UpdateField();
        }

        void starActiveTimer_Tick(object sender, EventArgs e)
        {
            //Возвращаем множитель очков к нормальному значению
            multiplier = 1;
            //Делаем текст с действием бонуса невидимым
            lblBonus.Visibility = Visibility.Hidden;
            starCooldownTimer.Start();
            starActiveTimer.Stop();
        }

        void starCooldownTimer_Tick(object sender, EventArgs e)
        {
            //Двигаем звезду на новое место
            star.move();
            //Добавляем изображение звезды на поле
            canvas1.Children.Add(star.image);
            //Обновляем положение звезды
            Canvas.SetTop(star.image, star.y);
            Canvas.SetLeft(star.image, star.x);
            starCooldownTimer.Stop();
            starDisappearTimer.Start();
        }

        void starDisappearTimer_Tick(object sender, EventArgs e)
        {
            //Убираем изображение звезды с поля
            canvas1.Children.Remove(star.image);
            //Перемещаем звезду за пределы игрового поля
            star.x = 0;
            star.y = 0;
            starDisappearTimer.Stop();
            starCooldownTimer.Start();
        }

        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                    head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                    head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                    head.direction = Head.Direction.RIGHT;
                    break;
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            
            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);
            // создаем новую звезду(бонус х10)
            star = new Star(snake);
            //Убираем текст с действием бонуса, если он остался на экране
            lblBonus.Visibility = Visibility.Hidden;
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);
            
            //запускаем таймеры движения и перезарядки звезды
            moveTimer.Start();
            starCooldownTimer.Start();
            UpdateField();

        }
        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Star : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Star(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/star.png")
            {
                m_snake = s;
            }

            public override void move()
            {
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }
    }
}
