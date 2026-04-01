namespace CSharpLearningDemo
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.listViewPets = new System.Windows.Forms.ListView();
            this.btnAddDog = new System.Windows.Forms.Button();
            this.btnAddCat = new System.Windows.Forms.Button();
            this.btnAddBird = new System.Windows.Forms.Button();
            this.btnSpeak = new System.Windows.Forms.Button();
            this.btnEat = new System.Windows.Forms.Button();
            this.btnTestInterface = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // listViewPets
            //
            this.listViewPets.FullRowSelect = true;
            this.listViewPets.GridLines = true;
            this.listViewPets.HideSelection = false;
            this.listViewPets.Location = new System.Drawing.Point(15, 30);
            this.listViewPets.Name = "listViewPets";
            this.listViewPets.Size = new System.Drawing.Size(360, 280);
            this.listViewPets.TabIndex = 0;
            this.listViewPets.UseCompatibleStateImageBehavior = false;
            //
            // btnAddDog
            //
            this.btnAddDog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnAddDog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddDog.ForeColor = System.Drawing.Color.White;
            this.btnAddDog.Location = new System.Drawing.Point(15, 30);
            this.btnAddDog.Name = "btnAddDog";
            this.btnAddDog.Size = new System.Drawing.Size(100, 35);
            this.btnAddDog.TabIndex = 1;
            this.btnAddDog.Text = "🐕 添加狗";
            this.btnAddDog.UseVisualStyleBackColor = false;
            this.btnAddDog.Click += new System.EventHandler(this.btnAddDog_Click);
            //
            // btnAddCat
            //
            this.btnAddCat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.btnAddCat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddCat.ForeColor = System.Drawing.Color.White;
            this.btnAddCat.Location = new System.Drawing.Point(130, 30);
            this.btnAddCat.Name = "btnAddCat";
            this.btnAddCat.Size = new System.Drawing.Size(100, 35);
            this.btnAddCat.TabIndex = 2;
            this.btnAddCat.Text = "🐱 添加猫";
            this.btnAddCat.UseVisualStyleBackColor = false;
            this.btnAddCat.Click += new System.EventHandler(this.btnAddCat_Click);
            //
            // btnAddBird
            //
            this.btnAddBird.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnAddBird.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddBird.ForeColor = System.Drawing.Color.White;
            this.btnAddBird.Location = new System.Drawing.Point(245, 30);
            this.btnAddBird.Name = "btnAddBird";
            this.btnAddBird.Size = new System.Drawing.Size(100, 35);
            this.btnAddBird.TabIndex = 3;
            this.btnAddBird.Text = "🐦 添加鸟";
            this.btnAddBird.UseVisualStyleBackColor = false;
            this.btnAddBird.Click += new System.EventHandler(this.btnAddBird_Click);
            //
            // btnSpeak
            //
            this.btnSpeak.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(39)))), ((int)(((byte)(176)))));
            this.btnSpeak.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSpeak.ForeColor = System.Drawing.Color.White;
            this.btnSpeak.Location = new System.Drawing.Point(15, 30);
            this.btnSpeak.Name = "btnSpeak";
            this.btnSpeak.Size = new System.Drawing.Size(160, 35);
            this.btnSpeak.TabIndex = 4;
            this.btnSpeak.Text = "📢 让所有宠物叫声";
            this.btnSpeak.UseVisualStyleBackColor = false;
            this.btnSpeak.Click += new System.EventHandler(this.btnSpeak_Click);
            //
            // btnEat
            //
            this.btnEat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnEat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEat.ForeColor = System.Drawing.Color.Black;
            this.btnEat.Location = new System.Drawing.Point(185, 30);
            this.btnEat.Name = "btnEat";
            this.btnEat.Size = new System.Drawing.Size(160, 35);
            this.btnEat.TabIndex = 5;
            this.btnEat.Text = "🍖 喂养所有宠物";
            this.btnEat.UseVisualStyleBackColor = false;
            this.btnEat.Click += new System.EventHandler(this.btnEat_Click);
            //
            // btnTestInterface
            //
            this.btnTestInterface.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(212)))));
            this.btnTestInterface.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestInterface.ForeColor = System.Drawing.Color.White;
            this.btnTestInterface.Location = new System.Drawing.Point(15, 75);
            this.btnTestInterface.Name = "btnTestInterface";
            this.btnTestInterface.Size = new System.Drawing.Size(330, 35);
            this.btnTestInterface.TabIndex = 6;
            this.btnTestInterface.Text = "🔧 测试接口 (多态演示)";
            this.btnTestInterface.UseVisualStyleBackColor = false;
            this.btnTestInterface.Click += new System.EventHandler(this.btnTestInterface_Click);
            //
            // btnRemove
            //
            this.btnRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.ForeColor = System.Drawing.Color.White;
            this.btnRemove.Location = new System.Drawing.Point(15, 120);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(330, 35);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.Text = "🗑️ 移除选中";
            this.btnRemove.UseVisualStyleBackColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(268, 30);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "🐾 C# 面向对象学习 Demo";
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(15, 320);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(360, 50);
            this.lblInfo.TabIndex = 9;
            this.lblInfo.Text = "提示：\n• 添加宠物后点击各按钮测试\n• 演示继承、多态、封装、接口";
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.listViewPets);
            this.groupBox1.Location = new System.Drawing.Point(12, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 320);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "宠物列表";
            //
            // groupBox2
            //
            this.groupBox2.Controls.Add(this.btnAddDog);
            this.groupBox2.Controls.Add(this.btnAddCat);
            this.groupBox2.Controls.Add(this.btnAddBird);
            this.groupBox2.Location = new System.Drawing.Point(420, 55);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(360, 80);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "添加宠物 (继承)";
            //
            // groupBox3
            //
            this.groupBox3.Controls.Add(this.btnSpeak);
            this.groupBox3.Controls.Add(this.btnEat);
            this.groupBox3.Controls.Add(this.btnTestInterface);
            this.groupBox3.Controls.Add(this.btnRemove);
            this.groupBox3.Location = new System.Drawing.Point(420, 155);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(360, 170);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "操作演示 (多态)";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 411);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "C# 面向对象学习 Demo";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView listViewPets;
        private System.Windows.Forms.Button btnAddDog;
        private System.Windows.Forms.Button btnAddCat;
        private System.Windows.Forms.Button btnAddBird;
        private System.Windows.Forms.Button btnSpeak;
        private System.Windows.Forms.Button btnEat;
        private System.Windows.Forms.Button btnTestInterface;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
