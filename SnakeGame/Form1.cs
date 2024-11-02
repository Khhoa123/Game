using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMPLib; // Import the Windows Media Player namespace

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        // Danh sách lưu trữ các điểm tạo thành thân rắn
        private List<Point> snake = new List<Point>();

        // Điểm đại diện cho vị trí của thức ăn
        private Point food;

        // Danh sách lưu trữ các điểm tạo thành chướng ngại vật
        private List<Point> obstacles = new List<Point>();

        // Biến lưu trữ điểm số của người chơi
        private int score = 0;

        // Biến điều khiển hướng di chuyển của rắn (0 nghĩa là không di chuyển)
        private int directionX = 0, directionY = 0;

        // Kích thước của mỗi đoạn rắn
        private int snakeSize = 20;

        // Kích thước của mỗi chướng ngại vật (nhỏ hơn rắn)
        private int obstacleSize = 5;

        // Biến để theo dõi trạng thái kết thúc trò chơi
        private bool isGameOver = false;

        // Biến xác định xem chướng ngại vật có được hiển thị hay không (dùng cho hiệu ứng nhấp nháy)
        private bool showObstacles = true;

        // Bộ đếm để kiểm soát tần suất nhấp nháy của chướng ngại vật
        private int blinkCounter = 0;

        // Hằng số xác định số lượng chướng ngại vật ban đầu và tối đa
        private const int initialObstacleCount = 3;
        private const int maxObstacleCount = 10;

        // Hình ảnh sử dụng cho đầu rắn, thân rắn, và thức ăn
        private Image snakeHeadImage;
        private Image snakeBodyImage;
        private Image foodImage;

        // Media player for background music
        private WindowsMediaPlayer backgroundMusicPlayer = new WindowsMediaPlayer();

        // Hàm khởi tạo cho form
        public Form1()
        {
            InitializeComponent(); // Khởi tạo các thành phần của form
            this.ClientSize = new Size(400, 400); // Đặt kích thước form là 400x400

            // Cài đặt bộ hẹn giờ để cập nhật trò chơi
            gameTimer.Tick += UpdateGame;
            gameTimer.Interval = 300; // Tốc độ trò chơi ban đầu

            // Tải các hình ảnh cho trò chơi
            snakeHeadImage = Image.FromFile("Resources/images.jpg");
            snakeBodyImage = Image.FromFile("Resources/snake_body.jpg");
            foodImage = Image.FromFile("Resources/apple.png");

            // Load and play background music
            backgroundMusicPlayer.URL = "D:\\cuaxin2\\SnakeGame\\SnakeGame\\Sound\\mr-23142.wav";
            backgroundMusicPlayer.settings.setMode("loop", true); // Loop the music
            backgroundMusicPlayer.controls.play();
        }

        // Phương thức bắt đầu hoặc khởi động lại trò chơi
        private void StartGame()
        {
            snake.Clear(); // Xóa thân rắn hiện tại
            snake.Add(new Point(100, 100)); // Đặt vị trí ban đầu của rắn
            directionX = 0; // Đặt hướng di chuyển thành không di chuyển
            directionY = 0;
            score = 0; // Đặt lại điểm số
            isGameOver = false; // Đặt trạng thái chưa kết thúc
            obstacles.Clear(); // Xóa các chướng ngại vật hiện tại
            GenerateObstacles(initialObstacleCount); // Tạo chướng ngại vật ban đầu

            gameTimer.Interval = 300; // Đặt lại tốc độ trò chơi
            gameTimer.Start(); // Bắt đầu hẹn giờ
            GenerateFood(); // Đặt thức ăn ban đầu lên bảng
            lblScore.Text = "Score: " + score; // Hiển thị điểm số ban đầu
        }

        // Phương thức tạo thức ăn ở vị trí ngẫu nhiên
        private void GenerateFood()
        {
            Random rand = new Random();
            int maxX = this.ClientSize.Width / snakeSize; // Tính toán vị trí x tối đa
            int maxY = this.ClientSize.Height / snakeSize; // Tính toán vị trí y tối đa
            food = new Point(rand.Next(0, maxX) * snakeSize, rand.Next(0, maxY) * snakeSize); // Tạo thức ăn tại một điểm ngẫu nhiên
        }

        // Phương thức tạo chướng ngại vật ở vị trí ngẫu nhiên
        private void GenerateObstacles(int count)
        {
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                Point obstacle;
                do
                {
                    // Tạo vị trí ngẫu nhiên cho chướng ngại vật
                    obstacle = new Point(rand.Next(0, this.ClientSize.Width / snakeSize) * snakeSize,
                                         rand.Next(0, this.ClientSize.Height / snakeSize) * snakeSize);
                } while (snake.Contains(obstacle) || food == obstacle || obstacles.Contains(obstacle)); // Đảm bảo không trùng lặp với rắn hoặc thức ăn

                obstacles.Add(obstacle); // Thêm chướng ngại vật vào danh sách
            }
        }

        // Phương thức cập nhật trạng thái trò chơi, được gọi bởi hẹn giờ
        private void UpdateGame(object sender, EventArgs e)
        {
            if (isGameOver)
                return; // Ngừng cập nhật nếu trò chơi đã kết thúc

            // Di chuyển các đoạn thân của rắn
            for (int i = snake.Count - 1; i > 0; i--)
            {
                snake[i] = snake[i - 1]; // Dịch chuyển từng đoạn đến vị trí của đoạn trước
            }

            // Di chuyển đầu rắn theo hướng hiện tại
            snake[0] = new Point(snake[0].X + directionX * snakeSize, snake[0].Y + directionY * snakeSize);

            // Kiểm tra va chạm với tường
            if (snake[0].X < 0 || snake[0].X >= this.ClientSize.Width || snake[0].Y < 0 || snake[0].Y >= this.ClientSize.Height)
            {
                GameOver(); // Kết thúc trò chơi nếu rắn chạm vào tường
                return;
            }

            // Kiểm tra va chạm với chính thân rắn
            for (int i = 1; i < snake.Count; i++)
            {
                if (snake[0] == snake[i])
                {
                    GameOver(); // Kết thúc trò chơi nếu rắn cắn vào thân
                    return;
                }
            }

            // Kiểm tra va chạm với chướng ngại vật
            foreach (var obstacle in obstacles)
            {
                if (snake[0] == obstacle)
                {
                    GameOver(); // Kết thúc trò chơi nếu rắn va vào chướng ngại vật
                    return;
                }
            }

            // Kiểm tra nếu rắn ăn thức ăn
            if (snake[0] == food)
            {
                // Thêm một đoạn mới vào thân rắn
                snake.Add(new Point(snake[snake.Count - 1].X, snake[snake.Count - 1].Y));
                score++; // Tăng điểm số
                lblScore.Text = "Score: " + score; // Cập nhật hiển thị điểm số
                GenerateFood(); // Tạo thức ăn mới

                // Tăng độ khó và thêm chướng ngại vật mỗi 5 điểm
                if (score % 5 == 0)
                {
                    if (obstacles.Count < maxObstacleCount)
                    {
                        GenerateObstacles(2); // Thêm nhiều chướng ngại vật hơn
                    }
                    gameTimer.Interval = Math.Max(50, gameTimer.Interval - 20); // Tăng tốc độ bằng cách giảm thời gian giữa các lần cập nhật
                    MoveObstacles(); // Thay đổi vị trí của chướng ngại vật hiện tại
                }
            }

            // Cập nhật trạng thái nhấp nháy cho chướng ngại vật
            blinkCounter++;
            if (blinkCounter >= 5) // Thay đổi trạng thái hiển thị mỗi 5 lần cập nhật
            {
                showObstacles = !showObstacles; // Đảo trạng thái hiển thị của chướng ngại vật
                blinkCounter = 0;
            }

            this.Invalidate(); // Vẽ lại form
        }

        // Phương thức di chuyển các chướng ngại vật đến vị trí mới
        private void MoveObstacles()
        {
            Random rand = new Random();
            List<Point> newObstacles = new List<Point>();

            foreach (var obstacle in obstacles)
            {
                Point newObstacle;
                do
                {
                    // Di chuyển chướng ngại vật đến vị trí mới gần đó
                    int newX = (obstacle.X + (rand.Next(-1, 2) * snakeSize) + this.ClientSize.Width) % this.ClientSize.Width;
                    int newY = (obstacle.Y + (rand.Next(-1, 2) * snakeSize) + this.ClientSize.Height) % this.ClientSize.Height;
                    newObstacle = new Point(newX, newY);
                } while (snake.Contains(newObstacle) || food == newObstacle || obstacles.Contains(newObstacle)); // Tránh trùng lặp

                newObstacles.Add(newObstacle); // Thêm vào danh sách mới
            }

            obstacles = newObstacles; // Thay thế danh sách cũ bằng danh sách mới
        }

        // Phương thức xử lý khi trò chơi kết thúc
        private void GameOver()
        {
            gameTimer.Stop(); // Dừng hẹn giờ
            isGameOver = true; // Đặt cờ kết thúc trò chơi
            MessageBox.Show("Game Over! Your score is: " + score); // Hiển thị thông báo kết thúc
            StartGame(); // Khởi động lại trò chơi
        }

        // Phương thức xử lý vẽ giao diện trò chơi
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            // Vẽ đầu rắn
            canvas.DrawImage(snakeHeadImage, new Rectangle(snake[0].X, snake[0].Y, snakeSize, snakeSize));
            for (int i = 1; i < snake.Count; i++)
            {
                // Vẽ thân rắn
                canvas.DrawImage(snakeBodyImage, new Rectangle(snake[i].X, snake[i].Y, snakeSize, snakeSize));
            }

            // Vẽ thức ăn
            canvas.DrawImage(foodImage, new Rectangle(food.X, food.Y, snakeSize, snakeSize));

            // Vẽ chướng ngại vật (khi chúng được hiển thị)
            if (showObstacles)
            {
                foreach (var obstacle in obstacles)
                {
                    // Vẽ hình tròn nhỏ cho mỗi chướng ngại vật
                    canvas.FillEllipse(Brushes.Red, new Rectangle(obstacle.X, obstacle.Y, obstacleSize, obstacleSize));
                }
            }
        }

        // Phương thức xử lý sự kiện khi nhấn phím
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Chuyển đổi hướng di chuyển của rắn dựa trên phím nhấn
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (directionY != 1) // Không cho phép di chuyển ngược lại hướng hiện tại
                    {
                        directionX = 0;
                        directionY = -1; // Di chuyển lên
                    }
                    break;
                case Keys.Down:
                    if (directionY != -1)
                    {
                        directionX = 0;
                        directionY = 1; // Di chuyển xuống
                    }
                    break;
                case Keys.Left:
                    if (directionX != 1)
                    {
                        directionX = -1; // Di chuyển sang trái
                        directionY = 0;
                    }
                    break;
                case Keys.Right:
                    if (directionX != -1)
                    {
                        directionX = 1; // Di chuyển sang phải
                        directionY = 0;
                    }
                    break;
            }
        }

        // Phương thức xử lý khi form được tải
        private void Form1_Load(object sender, EventArgs e)
        {
            StartGame(); // Bắt đầu trò chơi khi form được tải
        }
    }
}