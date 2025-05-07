# 环境依赖

* **开发平台**：**Visual Studio 2022**
* **目标框架**：**.NET 7.0**
* **数据库**：**PostgreSQL**
* **NuGet 包依赖**：
  `Microsoft.EntityFrameworkCore` v7.0.0
  `Microsoft.EntityFrameworkCore.Tools` v7.0.0
  `Npgsql.EntityFrameworkCore.PostgreSQL` v7.0.0
  `PdfSharpCore` v1.3.67
  `PuppeteerSharp` v20.0.0

# 数据库配置

项目使用 Entity Framework Core 连接 PostgreSQL 数据库。请在 `Data/IndividualDataContext.cs` 文件中的 `OnConfiguring` 方法中，修改连接字符串以匹配您的数据库设置：`optionsBuilder.UseNpgsql("Host=;Database=;Username=;Password="); `

# 编译与运行

1. **克隆项目**

2. **导入数据用例：**在本地PostgreSql数据库中导入数据样本family.sql

3. **还原 NuGet 包**：
   在 Visual Studio 中，依次点击“工具” > “NuGet 包管理器” > “程序包管理器控制台”，然后安装环境依赖中的NuGet包。

4. **配置数据库连接字符串**

5. **构建项目**：
   点击“生成” > “生成解决方案”。

6. **运行应用**：
   点击“调试”。

   # 注意事项

* **数据库连接**：确保 PostgreSQL 服务正在运行，且连接字符串中的主机、数据库名、用户名和密码正确无误。
* **Chromium 下载**：首次使用 `PuppeteerSharp` 时，会自动下载 Chromium 浏览器，需确保网络连接正常。
* **PDF 文件管理**：系统会先生成多个单页 PDF，再合并为一个总 PDF。合并后，系统会自动删除单页 PDF 文件，以节省存储空间。
* **测试用例**：使用”黄冈靖氏家谱“进行测试，其他家谱数据样本存在错误或数据量少。
