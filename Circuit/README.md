# Circuit - 高性能直流电路求解引擎

这是一个轻量级、超高性能的直流电路分析与求解库，采用现代工业级 **改进节点电压法 (Modified Nodal Analysis, MNA)** 实现，专为电路仿真及电路益智类游戏设计。

---

## 核心特性

- **现代 MNA 算法引擎**：摒弃传统繁重的回路矩阵拓扑搜索（生成树与基本回路），直接使用支路导纳盖印（Stamping）构建节点方程，单次计算达微秒级（< 250 µs）。
- **并查集快速连通性划分**：采用近乎 $O(1)$ 的并查集对多独立连通电路进行切分，无图对象拷贝开销。
- **支持常见电路元件**：
  - 电阻（Resistance）
  - 理想电源与带内阻电源（Battery）
  - 导线（Wire）
  - 电流表与理想电压表（Ammeter / Voltmeter）
  - 复合支路（Complex Branch）
- **开箱即用**：采用最新 SDK 风格工程配置，支持 .NET 10 / .NET Standard。

---

## 性能表现

在 Release 模式下的实测求解表现：

| 电路规模 | 传统拓扑回路法 | 现代 MNA 引擎 | 加速比 |
| :--- | :--- | :--- | :--- |
| **50 支路** (25 节点) | ~ 2.0 ms | **0.099 ms (99 µs)** | **20x+** |
| **120 支路** (50 节点) | ~ 7.8 ms | **0.253 ms (253 µs)** | **30x+** |

---

## 快速使用

```csharp
using Circuit;

// 1. 实例化电路
Circuit c = new Circuit();

// 2. 添加支路元件 (节点编号, 节点编号, 元件属性)
// 添加 12V 电池（内阻 3Ω）
c.Add(0, 2, new Branch() { R = 3, E = -12, Name = "i1" });
// 添加普通电阻 3Ω
c.Add(0, 2, new Branch() { R = 3, Name = "i2" });
// 添加电阻 2Ω
c.Add(1, 2, new Branch() { R = 2, Name = "i3" });
// 添加 8V 电池（内阻 2Ω）
c.Add(2, 1, new Branch() { R = 2, E = 8, Name = "i4" });
// 添加电阻 1.5Ω
c.Add(0, 1, new Branch() { R = 1.5, Name = "i5" });

// 3. 求解电路
c.Solve();

// 4. 打印结果
c.Print();
```

---

## 许可证

Open Source C# Code © Nobody All Rights Reserved.
