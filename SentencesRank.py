from nltk.tokenize import sent_tokenize
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from transformers import AutoTokenizer, AutoModel
import pandas as pd
import numpy as np
import torch, json, os
import torch.nn.functional as F


dir = r'D:\TestDir'
if not os.path.exists(dir):
    os.makedirs(dir)

def ReadJson(jsonPath):
    with open(jsonPath, 'r', encoding='utf-8') as json_file:
        try:
            return json.load(json_file)
        except json.JSONDecodeError as e:
            raise ValueError(f"無法解析 JSON 檔案: {e}")

def ReadFile(filePath):
    try:
        with open(filePath, mode='r', encoding="utf-8") as file:
            fileContent = file.read()

        return fileContent
    except Exception as e:
        print(f"讀取檔案時發生錯誤: {e}")
    
def WriteFile(fileName, data):
    try:
        filePath = os.path.join(dir, fileName)
    
        with open(filePath, 'w', encoding='utf-8') as json_file:
            json.dump(data, json_file, ensure_ascii=False, indent=4)

        return filePath
    except Exception as e:
        print(f"寫入檔案時發生錯誤: {e}")

def WriteTextFile(fileName, data):
    try:
        filePath = os.path.join(dir, fileName)

        if isinstance(data, list):
            data = '\n'.join(data)

        with open(filePath, 'w', encoding='utf-8') as text_file:
            text_file.write(data)

        return filePath
    except Exception as e:
        print(f"寫入檔案時發生錯誤: {e}")

def RankSentencesWithTitle(text, title, title_weight=0.5):
    # 分句
    sentences = sent_tokenize(text)

    # 向量化
    vectorizer = TfidfVectorizer()
    X = vectorizer.fit_transform(sentences)
    
    doc_vector = np.mean(X, axis=0)

    title_vector = vectorizer.transform([title])

    # 計算相似度
    content_similarity = cosine_similarity(X, np.asarray(doc_vector.reshape(1, -1)))
    title_similarity = cosine_similarity(X, title_vector)

    # 加權平均
    similarity = title_similarity * title_weight + content_similarity * (1 - title_weight)

    # 建立一個字典，key為句子索引，value為相似度
    similarity_dict = dict(zip(range(len(sentences)), similarity.flatten()))

    # 排序並取前5
    top_5 = sorted(similarity_dict.items(), key=lambda x: x[1], reverse=True)[:5]
    top_sentences = [{score: sentences[i]} for (i, score) in top_5]

    return top_sentences


def RankSentencesWithBERT(text, title):
    # 初始化 tokenizer 和 model
    tokenizer = AutoTokenizer.from_pretrained('bert-base-uncased')
    model = AutoModel.from_pretrained('bert-base-uncased')

    # 將文本分割成句子
    sentences = sent_tokenize(text)

    # 對所有句子進行批次編碼
    inputs = tokenizer(sentences, padding=True, truncation=True, return_tensors='pt')

    # 獲取標題的 embedding
    title_inputs = tokenizer(title, return_tensors='pt')
    with torch.no_grad():
        title_outputs = model(**title_inputs)
        title_embedding = title_outputs.pooler_output  # (1, hidden_size)

    # 獲取所有句子的 embeddings (批次處理)
    with torch.no_grad():
        outputs = model(**inputs)
        sentence_embeddings = outputs.pooler_output  # (num_sentences, hidden_size)

    # 將 sentence_embeddings 調整為 (num_sentences, 1, hidden_size)
    sentence_embeddings = sentence_embeddings.unsqueeze(1)  # (num_sentences, 1, hidden_size)

    # 計算每個句子與標題之間的餘弦相似度
    similarity_scores = F.cosine_similarity(sentence_embeddings, title_embedding.unsqueeze(0), dim=2)  # (num_sentences, 1)
    # print(similarity_scores)
    # 將相似度壓縮為一維張量
    similarity_scores = similarity_scores.squeeze(1)  # (num_sentences)

    # 將句子和相似度配對，並按分數排序
    ranked_sentences = sorted(zip(sentences, similarity_scores), key=lambda x: x[1], reverse=True)
    top_sentences = [{score: sentence} for sentence, score in ranked_sentences]

    # 格式化前五個相似度最高的句子
    top_5 = [{score.item(): f'{sentence}'} for sentence, score in ranked_sentences[:5]]

    return top_5

def RankSelectedFiles(filteredData):
    resultList = []
    for data in filteredData:
        data['SentenceVector'] = RankSentencesWithTitle(data['content'], data['title'])
        data['SentenceBERT'] = RankSentencesWithBERT(data['content'], data['title'])
        resultList.append(data)
    return resultList

def StartProcess():
    selectFiles = os.path.join(dir, "selectedItems.txt")  
    if not os.path.exists(selectFiles):
        print(f"檔案 {selectFiles} 不存在。")
        return f"檔案 {selectFiles} 不存在。"
    
    content = ReadFile(selectFiles)   
    filePaths = content.split('&')
    # print(filePaths)
    jsonPath = os.path.join(dir, "defaultFiles.json")
    if not os.path.exists(jsonPath):
        print(f"檔案不存在: {jsonPath}")
    defaultData = ReadJson(jsonPath)
    
    filteredData = [item for item in defaultData if item["filePath"] in filePaths]
    # print(filteredData)
    result = RankSelectedFiles(filteredData)
    
    filePath = WriteFile("sentencesRank.json", result)

    print(filePath)

StartProcess()
