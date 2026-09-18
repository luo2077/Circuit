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

## 示例

下面是一个需要求解的电路：
![image](https://github.com/luo2077/Circuit/blob/main/img/eg_circuit.png)

在代码中构造电路类，插入所有支路，包括支路的已知属性（电阻、电动势）：
![image](https://github.com/luo2077/Circuit/blob/main/img/main.png)

运行程序，在控制台即可查看求解结果：
![image](https://github.com/luo2077/Circuit/blob/main/img/output.png)

---

## 许可证

Open Source C# Code © Nobody All Rights Reserved.
