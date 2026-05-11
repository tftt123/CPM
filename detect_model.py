#!/usr/bin/env python3
"""
模型版本探测脚本
支持：OpenAI 兼容 API、Kimi API、Anthropic API
使用方法：
  1. 填入下方的 API_KEY 和 BASE_URL
  2. 运行：python detect_model.py
"""

import json
import os
import sys

# ====================== 请修改以下配置 ======================

API_KEY = os.getenv("API_KEY", "sk-your-api-key-here")
BASE_URL = os.getenv("BASE_URL", "https://api.moonshot.cn/v1")  # Kimi 默认示例
MODEL_NAME = os.getenv("MODEL_NAME", "kimi-for-coding")  # 你配置的模型名

# 如果是 Anthropic API，取消下面注释并修改：
# BASE_URL = "https://api.anthropic.com/v1"
# MODEL_NAME = "claude-sonnet-4-20250514"

# 如果是 OpenAI/Azure/中转代理，修改 BASE_URL 即可
# BASE_URL = "https://api.openai.com/v1"

# ==========================================================


def detect_openai_compatible():
    """探测 OpenAI 兼容格式的 API（包括 Kimi、Azure、中转代理）"""
    try:
        import openai
    except ImportError:
        print("[错误] 缺少 openai 库，正在尝试安装...")
        os.system(f"{sys.executable} -m pip install openai -q")
        import openai

    client = openai.OpenAI(api_key=API_KEY, base_url=BASE_URL)

    print("=" * 60)
    print("正在探测 OpenAI 兼容 API...")
    print(f"BASE_URL: {BASE_URL}")
    print(f"请求模型: {MODEL_NAME}")
    print("=" * 60)

    # 1. 尝试获取模型列表
    try:
        models = client.models.list()
        print("\n[1] 可用模型列表（前 10 个）：")
        for i, m in enumerate(models.data[:10]):
            marker = " <-- 你配置的模型" if m.id == MODEL_NAME else ""
            print(f"    {i+1}. {m.id}{marker}")
    except Exception as e:
        print(f"\n[1] 获取模型列表失败（部分平台不支持）: {e}")

    # 2. 发起一个简单请求，捕获响应中的 model 字段
    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[{"role": "user", "content": "你好，请只回复一个单词：Hello"}],
            max_tokens=10,
            stream=False,
        )
        print(f"\n[2] 实际响应模型字段: {response.model}")
        print(f"    请求模型: {MODEL_NAME}")
        print(f"    是否一致: {'✅ 是' if response.model == MODEL_NAME else '⚠️ 否（可能被路由/映射）'}")

        # 3. 特征问题探测
        probe_response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[{
                "role": "user",
                "content": (
                    "请严格按以下格式回答，不要添加任何解释：\n"
                    "1. 你的名字/模型身份：\n"
                    "2. 你的知识截止日期：\n"
                    "3. 你的开发商/公司："
                )
            }],
            max_tokens=200,
            temperature=0,
        )
        print(f"\n[3] 模型自我声明身份：\n{probe_response.choices[0].message.content}")

    except Exception as e:
        print(f"\n[2] 请求失败: {e}")
        return False

    return True


def detect_anthropic():
    """探测 Anthropic 原生 API"""
    try:
        import anthropic
    except ImportError:
        print("[错误] 缺少 anthropic 库，正在尝试安装...")
        os.system(f"{sys.executable} -m pip install anthropic -q")
        import anthropic

    client = anthropic.Anthropic(api_key=API_KEY)

    print("=" * 60)
    print("正在探测 Anthropic API...")
    print("=" * 60)

    try:
        response = client.messages.create(
            model=MODEL_NAME,
            max_tokens=200,
            temperature=0,
            messages=[{
                "role": "user",
                "content": (
                    "请严格按以下格式回答，不要添加任何解释：\n"
                    "1. 你的名字/模型身份：\n"
                    "2. 你的知识截止日期：\n"
                    "3. 你的开发商/公司："
                )
            }]
        )
        print(f"\n[响应] 模型自我声明身份：\n{response.content[0].text}")
        print(f"\n[信息] Anthropic API 的 model 字段就是你请求的: {MODEL_NAME}")
    except Exception as e:
        print(f"\n[错误] Anthropic API 请求失败: {e}")
        return False

    return True


def curl_probe():
    """如果以上都失败，用原始 curl/requests 探测响应头"""
    import urllib.request
    import ssl

    print("\n" + "=" * 60)
    print("使用原始 HTTP 请求探测响应头...")
    print("=" * 60)

    payload = json.dumps({
        "model": MODEL_NAME,
        "messages": [{"role": "user", "content": "hi"}],
        "max_tokens": 5
    }).encode()

    req = urllib.request.Request(
        f"{BASE_URL}/chat/completions",
        data=payload,
        headers={
            "Authorization": f"Bearer {API_KEY}",
            "Content-Type": "application/json"
        },
        method="POST"
    )

    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE

    try:
        with urllib.request.urlopen(req, context=ctx, timeout=30) as resp:
            print(f"\nHTTP 状态码: {resp.status}")
            print("响应头:")
            for k, v in resp.headers.items():
                if any(x in k.lower() for x in ["model", "version", "x-request-id", "cf-ray", "x-"]):
                    print(f"  {k}: {v}")

            body = json.loads(resp.read())
            print(f"\n响应体 model 字段: {body.get('model', 'N/A')}")
            if 'usage' in body:
                print(f"Token 使用: {body['usage']}")
    except Exception as e:
        print(f"原始请求也失败了: {e}")


if __name__ == "__main__":
    if API_KEY.startswith("sk-your"):
        print("[警告] 你还没有配置 API_KEY！")
        print("请修改脚本第 18 行的 API_KEY，或设置环境变量：")
        print("  export API_KEY='sk-...'")
        print("  export BASE_URL='https://...'")
        sys.exit(1)

    success = False

    # 根据 URL 特征判断 API 类型
    if "anthropic" in BASE_URL.lower():
        success = detect_anthropic()
    else:
        success = detect_openai_compatible()

    if not success:
        curl_probe()

    print("\n" + "=" * 60)
    print("探测完成。如果你使用的是中转/聚合平台，实际调用的模型")
    print"可能与配置的 MODEL_NAME 不一致，以上结果仅供参考。")
    print("=" * 60)
