using System.Windows.Forms;
using CSharpLearningDemo.Models;

namespace CSharpLearningDemo
{
    /// <summary>
    /// 主窗体 - 演示 C# 面向对象特性
    ///
    /// 本 Demo 演示的核心概念：
    /// 1. 封装 (Encapsulation)
    ///    - Animal 类的属性封装了私有字段
    ///    - 属性可以添加验证逻辑
    ///
    /// 2. 继承 (Inheritance)
    ///    - Dog, Cat, Bird 继承自 Animal
    ///    - 子类自动拥有父类的属性和方法
    ///
    /// 3. 多态 (Polymorphism)
    ///    - 同一个 Speak() 方法，不同子类有不同实现
    ///    - 运行时自动调用对应的实现
    ///
    /// 4. 接口 (Interface)
    ///    - IEat, ISpeak, IFlyable 接口
    ///    - 定义行为规范，实现类提供具体实现
    /// </summary>
    public partial class MainForm : Form
    {
        // ==================== 字段 ====================

        /// <summary>
        /// 宠物管理器 - 管理所有宠物
        /// </summary>
        private PetManager _petManager = new PetManager();

        // ==================== 构造函数 ====================

        public MainForm()
        {
            InitializeComponent();
            InitializeListView();
        }

        // ==================== 初始化 ====================

        /// <summary>
        /// 初始化 ListView
        /// </summary>
        private void InitializeListView()
        {
            // 添加列
            listViewPets.Columns.Add("类型", 60);
            listViewPets.Columns.Add("名字", 100);
            listViewPets.Columns.Add("年龄", 50);
            listViewPets.Columns.Add("颜色", 80);
            listViewPets.Columns.Add("特性", 100);

            // 设置视图模式
            listViewPets.View = View.Details;
        }

        /// <summary>
        /// 刷新列表显示
        /// </summary>
        private void RefreshListView()
        {
            listViewPets.Items.Clear();

            var pets = _petManager.GetAllPets();
            int index = 0;

            foreach (var pet in pets)
            {
                ListViewItem item = new ListViewItem();

                // 根据类型显示不同图标
                if (pet is Dog)
                    item.Text = "🐕";
                else if (pet is Cat)
                    item.Text = "🐱";
                else if (pet is Bird)
                    item.Text = "🐦";
                else
                    item.Text = "🐾";

                // 获取宠物信息
                string[] info = GetPetInfo(pet);
                item.SubItems.Add(info[0]); // 名字
                item.SubItems.Add(info[1]); // 年龄
                item.SubItems.Add(info[2]); // 颜色
                item.SubItems.Add(info[3]); // 特性

                // 保存索引
                item.Tag = index;

                listViewPets.Items.Add(item);
                index++;
            }
        }

        /// <summary>
        /// 获取宠物信息 - 多态的体现
        /// </summary>
        private string[] GetPetInfo(Animal pet)
        {
            string[] info = new string[4];
            info[0] = pet.Name;
            info[1] = pet.Age.ToString();
            info[2] = pet.Color;

            // 根据类型显示不同特性
            if (pet is Dog dog)
                info[3] = $"品种: {dog.Breed}";
            else if (pet is Cat cat)
                info[3] = cat.IsLazy ? "懒惰" : "活泼";
            else if (pet is Bird bird)
                info[3] = bird.CanFly ? "能飞" : "不能飞";
            else
                info[3] = "";

            return info;
        }

        // ==================== 添加宠物按钮事件 ====================

        /// <summary>
        /// 添加狗 - 继承自 Animal
        /// </summary>
        private void btnAddDog_Click(object sender, System.EventArgs e)
        {
            // 创建狗对象 - 继承自 Animal
            // 使用构造函数初始化属性
            Dog dog = new Dog("旺财", 3, "黄色", "金毛");

            // 添加到管理器
            _petManager.AddPet(dog);

            // 刷新显示
            RefreshListView();

            MessageBox.Show("🐕 添加了小狗：旺财\n品种: 金毛", "成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 添加猫 - 继承自 Animal
        /// </summary>
        private void btnAddCat_Click(object sender, System.EventArgs e)
        {
            // 创建猫对象
            Cat cat = new Cat("咪咪", 2, "白色", isLazy: true);

            _petManager.AddPet(cat);
            RefreshListView();

            MessageBox.Show("🐱 添加了小猫：咪咪\n特点: 懒惰", "成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 添加鸟 - 继承自 Animal, 实现 IFlyable 接口
        /// </summary>
        private void btnAddBird_Click(object sender, System.EventArgs e)
        {
            // 创建鸟对象 - 能飞的鹦鹉
            Bird bird = new Bird("小鹦", 1, "绿色", canFly: true);

            _petManager.AddPet(bird);
            RefreshListView();

            MessageBox.Show("🐦 添加了小鸟：小鹦\n特点: 能飞", "成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==================== 操作按钮事件 ====================

        /// <summary>
        /// 让所有宠物叫声 - 演示【多态】
        /// 关键点：这里只调用 Speak()，但运行时
        /// 会根据实际类型调用对应子类的 Speak()
        /// </summary>
        private void btnSpeak_Click(object sender, System.EventArgs e)
        {
            if (_petManager.Count == 0)
            {
                MessageBox.Show("请先添加宠物！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 多态演示：调用 MakeAllSpeak
            // 内部会遍历所有宠物，调用各自的 Speak() 方法
            string result = "===== 所有宠物叫声 =====\n";

            var pets = _petManager.GetAllPets();
            foreach (var pet in pets)
            {
                // 多态：运行时决定调用哪个 Speak()
                pet.Speak();
                result += $"{pet.Name}: {GetSpeakSound(pet)}\n";
            }

            MessageBox.Show(result, "多态演示 - 叫声",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 获取叫声描述
        /// </summary>
        private string GetSpeakSound(Animal pet)
        {
            if (pet is Dog) return "汪汪汪！";
            if (pet is Cat) return "喵喵喵~";
            if (pet is Bird bird)
                return bird.CanFly ? "吱吱叫~" : "咕咕叫~";
            return "...";
        }

        /// <summary>
        /// 喂养所有宠物 - 演示【多态】
        /// </summary>
        private void btnEat_Click(object sender, System.EventArgs e)
        {
            if (_petManager.Count == 0)
            {
                MessageBox.Show("请先添加宠物！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string result = "===== 喂养宠物 =====\n";

            var pets = _petManager.GetAllPets();
            foreach (var pet in pets)
            {
                pet.Eat();
                result += $"{pet.Name} 正在吃 {GetFood(pet)}\n";
            }

            MessageBox.Show(result, "多态演示 - 吃饭",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 获取食物
        /// </summary>
        private string GetFood(Animal pet)
        {
            if (pet is Dog) return "狗粮";
            if (pet is Cat) return "鱼罐头";
            if (pet is Bird) return "谷子";
            return "食物";
        }

        /// <summary>
        /// 测试接口 - 演示【接口】和【多态】
        /// </summary>
        private void btnTestInterface_Click(object sender, System.EventArgs e)
        {
            if (_petManager.Count == 0)
            {
                MessageBox.Show("请先添加宠物！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string result = "===== 接口测试 =====\n\n";

            var pets = _petManager.GetAllPets();
            foreach (var pet in pets)
            {
                result += $"【{pet.Name}】\n";

                // 使用 is 检查是否实现了接口，然后调用
                if (pet is ISpeak speakable)
                {
                    result += $"  接口(ISpeak): {speakable.GetSoundDescription()}\n";
                }

                if (pet is IEat eatable)
                {
                    result += $"  接口(IEat): {eatable.GetFoodPreference()}\n";
                }

                // 只有鸟类实现了 IFlyable
                if (pet is IFlyable flyable)
                {
                    result += $"  接口(IFlyable): ";
                    flyable.Fly();
                    result += $"  飞行高度: {flyable.FlightAltitude}米\n";
                }

                result += "\n";
            }

            MessageBox.Show(result, "接口测试",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void btnRemove_Click(object sender, System.EventArgs e)
        {
            if (listViewPets.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要移除的宠物！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取选中项的索引
            int index = (int)listViewPets.SelectedItems[0].Tag;

            // 移除
            _petManager.RemovePet(index);

            // 刷新
            RefreshListView();

            MessageBox.Show("移除成功！", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
