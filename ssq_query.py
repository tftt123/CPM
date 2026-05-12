#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
双色球开奖信息查询工具
实现 FetchURL + SearchWeb 的功能
"""

import requests
from bs4 import BeautifulSoup
import re
import json
from urllib.parse import quote


class SsqQuery:
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            "Accept-Language": "zh-CN,zh;q=0.9,en;q=0.8",
        })
        import sys
        import io
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    
    def fetch_url(self, url: str) -> str:
        """
        模拟 FetchURL 工具：获取网页内容
        """
        print(f"[FetchURL] 正在访问: {url}")
        try:
            response = self.session.get(url, timeout=15)
            response.raise_for_status()
            response.encoding = response.apparent_encoding or 'utf-8'
            print(f"[FetchURL] 成功获取，内容长度: {len(response.text)} 字符\n")
            return response.text
        except Exception as e:
            print(f"[FetchURL] 错误: {e}")
            return ""
    
    def search_web(self, query: str, limit: int = 5) -> list:
        """
        模拟 SearchWeb 工具：使用 DuckDuckGo 搜索（无需 API Key）
        """
        print(f"[SearchWeb] 搜索关键词: '{query}'")
        
        # 使用 DuckDuckGo HTML 版本进行搜索
        search_url = f"https://html.duckduckgo.com/html/?q={quote(query)}"
        
        try:
            response = self.session.get(search_url, timeout=15)
            response.encoding = 'utf-8'
            soup = BeautifulSoup(response.text, 'html.parser')
            
            results = []
            for result in soup.select('.result')[:limit]:
                title_elem = result.select_one('.result__title')
                snippet_elem = result.select_one('.result__snippet')
                url_elem = result.select_one('.result__url')
                
                if title_elem and snippet_elem:
                    results.append({
                        'title': title_elem.get_text(strip=True),
                        'snippet': snippet_elem.get_text(strip=True),
                        'url': url_elem.get_text(strip=True) if url_elem else ''
                    })
            
            print(f"[SearchWeb] 找到 {len(results)} 条结果\n")
            return results
            
        except Exception as e:
            print(f"[SearchWeb] 错误: {e}")
            return []
    
    def extract_ssq_info(self, html_content: str) -> dict:
        """
        从福彩官网页面提取双色球开奖信息
        """
        info = {
            '期号': '',
            '开奖日期': '',
            '销售金额': '',
            '奖池金额': '',
            '一等奖': {},
            '二等奖': {},
            '三等奖': {},
            '开奖号码': {'红球': [], '蓝球': ''}
        }
        
        # 尝试提取销售金额
        sale_match = re.search(r'本期销售金额[：:]\s*([\d,]+)\s*元', html_content)
        if sale_match:
            info['销售金额'] = sale_match.group(1)
        
        # 尝试提取奖池金额
        pool_match = re.search(r'奖池累计金额[：:]\s*([\d,]+)\s*元', html_content)
        if pool_match:
            info['奖池金额'] = pool_match.group(1)
        
        # 尝试提取开奖日期
        date_match = re.search(r'开奖日期[：:]\s*(\d{4}-\d{2}-\d{2})', html_content)
        if date_match:
            info['开奖日期'] = date_match.group(1)
        
        # 提取一、二、三等奖信息（从表格或文本中）
        # 一等奖
        first_match = re.search(r'一等奖[\s,;，；]*?(\d+)[\s,;，；]*?([\d,]+)', html_content)
        if first_match:
            info['一等奖'] = {
                '注数': first_match.group(1),
                '单注奖金': first_match.group(2)
            }
        
        # 二等奖
        second_match = re.search(r'二等奖[\s,;，；]*?(\d+)[\s,;，；]*?([\d,]+)', html_content)
        if second_match:
            info['二等奖'] = {
                '注数': second_match.group(1),
                '单注奖金': second_match.group(2)
            }
        
        # 三等奖
        third_match = re.search(r'三等奖[\s,;，；]*?(\d+)[\s,;，；]*?([\d,]+)', html_content)
        if third_match:
            info['三等奖'] = {
                '注数': third_match.group(1),
                '单注奖金': third_match.group(2)
            }
        
        return info
    
    def query_ssq_period(self, period: str) -> dict:
        """
        查询指定期号的双色球开奖信息
        """
        print(f"=== 开始查询双色球第{period}期 ===\n")
        
        # 步骤1: 尝试直接访问中彩网（可能无法获取动态内容）
        direct_url = f"https://www.zhcw.com/kjxx/ssq/"
        print("步骤1: 尝试直接访问中彩网...")
        html = self.fetch_url(direct_url)
        
        # 步骤2: 使用搜索找到具体开奖页面
        print("步骤2: 使用搜索引擎查找开奖详情...")
        search_query = f"双色球第{period}期 开奖公告 销售金额 一等奖"
        search_results = self.search_web(search_query, limit=5)
        
        all_info = {}
        
        # 步骤3: 访问搜索结果中的页面，提取详细信息
        print("步骤3: 访问搜索结果页面提取详细信息...")
        for idx, result in enumerate(search_results, 1):
            print(f"\n--- 结果 {idx}: {result['title']} ---")
            print(f"URL: {result['url']}")
            print(f"摘要: {result['snippet'][:100]}...")
            
            # 尝试从摘要中提取信息
            info = self.extract_ssq_info(result['snippet'])
            
            # 如果有关键信息，合并到结果中
            if info['销售金额']:
                all_info['销售金额'] = info['销售金额']
            for prize in ['一等奖', '二等奖', '三等奖']:
                if info[prize].get('单注奖金'):
                    all_info[prize] = info[prize]
        
        # 步骤4: 尝试直接访问一些已知的福彩信息页面
        print("\n步骤4: 尝试访问官方福彩信息源...")
        
        # 尝试访问徐州福彩的页面（之前搜索到的可靠来源）
        xzflcp_url = f"http://www.xzflcp.com/html/2025/202510052662.html"
        xz_html = self.fetch_url(xzflcp_url)
        if xz_html:
            xz_info = self.extract_ssq_info(xz_html)
            if xz_info['销售金额']:
                all_info['销售金额'] = xz_info['销售金额']
            for prize in ['一等奖', '二等奖', '三等奖']:
                if xz_info[prize].get('单注奖金'):
                    all_info[prize] = xz_info[prize]
        
        print(f"\n=== 双色球第{period}期查询结果 ===")
        return all_info
    
    def print_result(self, period: str, info: dict):
        """
        格式化输出查询结果
        """
        print("\n" + "=" * 50)
        print(f"双色球第{period}期开奖信息")
        print("=" * 50)
        
        if '销售金额' in info:
            print(f"\n【总销售额】")
            sale = info['销售金额'].replace(',', '')
            print(f"  {int(sale):,} 元")
        
        print(f"\n【一、二、三等奖单注奖金】")
        for prize_name in ['一等奖', '二等奖', '三等奖']:
            if prize_name in info:
                prize = info[prize_name]
                amount = prize.get('单注奖金', '').replace(',', '')
                if amount:
                    print(f"  {prize_name}: {int(amount):,} 元")
                else:
                    print(f"  {prize_name}: 信息未获取")
        
        print("=" * 50)


def main():
    """
    主函数：查询双色球第2025114期
    """
    query = SsqQuery()
    period = "2025114"
    
    # 执行查询
    result = query.query_ssq_period(period)
    
    # 输出结果
    query.print_result(period, result)
    
    return result


if __name__ == "__main__":
    main()
