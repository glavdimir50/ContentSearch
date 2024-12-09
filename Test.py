import sys, os, json, string, Levenshtein
from bs4 import BeautifulSoup
from collections import defaultdict, Counter
from gensim.models import Word2Vec
from nltk.tokenize import sent_tokenize
from nltk.tokenize import word_tokenize
from nltk.corpus import stopwords
from nltk.stem import SnowballStemmer
from nltk import FreqDist
from sklearn.cluster import KMeans
from sklearn.manifold import TSNE
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.preprocessing import normalize
import matplotlib.pyplot as plt
import numpy as np

# arguments = sys.argv[1].split('|')

dir = r'D:\TestDir'
if not os.path.exists(dir):
    os.makedirs(dir)

#region Word2Vec
# 1. 訓練 Word2Vec 模型
def TrainWord2vecModel(documents, sg, vector_size=100, window=5, min_count=2, model_path=None):
    model = Word2Vec(sentences=documents, vector_size=vector_size, window=window, min_count=min_count, sg=sg)
    if model_path:
        model.save(model_path)
    return model

# 2. 加載 Word2Vec 模型
def LoadWord2vecModel(model_path):
    return Word2Vec.load(model_path)

# 3. 文本預處理與分詞（返回內容與文件名稱）
# def PreprocessDocuments(jsonData, queryWords):
#     stop_words = set(stopwords.words('english'))
#     documents = []
#     doc_names = []  # 儲存文件名稱
#     for fileData in jsonData:
#         content = fileData['content']
#         if content:
#             for keyword in queryWords:
#                 tokens = word_tokenize(content.lower())
#                 tokens = [word for word in tokens if word.isalpha() and word not in stop_words]
#                 documents.append(tokens)
#                 doc_names.append(fileData['filePath'])  # 添加文件名稱
#     return documents, doc_names
def PreprocessDocuments(jsonData, keywords, matchMode):
    # 篩選文章的邏輯
    stop_words = set(stopwords.words('english'))
    filtered_documents = []
    for fileData in jsonData:  # 假設 all_documents 是所有可搜尋的文件
        documents = []
        content = fileData["content"]
        if content:
            tokens = word_tokenize(content.lower())
            tokens = [word for word in tokens if word.isalpha() and word not in stop_words]
            documents.append(tokens)
            if matchMode == 'AND':
                if all(keyword in content for keyword in keywords):
                    filtered_documents.append({
                        "filePath": fileData["filePath"],
                        "documents": documents,
                        "content": content
                    })
            elif matchMode == 'OR':
                if any(keyword in content for keyword in keywords):
                    filtered_documents.append({
                        "filePath": fileData["filePath"],
                        "documents": documents,
                        "content": content
                    })
        
    return json.dumps(filtered_documents)  # 回傳 JSON 格式的結果

# 4. 查詢擴展
# def ExpandQuery(query_word, model, top_n=100):
#     if query_word in model.wv:
#         similar_words = model.wv.most_similar(query_word, topn=top_n)
#         # 將查詢結果轉換為 JSON 格式
#         result = [{word:str(float(similarity))} for word, similarity in similar_words]
#         return result
#     return []
def ExpandQuery(query_words, model, top_n=100):
    result = {}
    for word in query_words:
        if word in model.wv:
            similar_words = model.wv.most_similar(word, topn=top_n)
            result[word] = [{similar_word: str(float(similarity))} for similar_word, similarity in similar_words]
        else:
            result[word] = []
    return result

# 5. 文檔向量化
def VectorizeDocuments(doc, model):
    vectors = [model.wv[word] for word in doc if word in model.wv]
    return np.mean(vectors, axis=0) if vectors else np.zeros(model.vector_size)

# 6. 語意相似排序，顯示文件名稱
# def RankDocuments(query_word, documents, model, doc_names):
#     query_vector = model.wv[query_word] if query_word in model.wv else None
#     if query_vector is None:
#         print(f"The word '{query_word}' is not in the vocabulary.")
#         return []#json.dumps([], indent=2, ensure_ascii=False)
    
#     doc_vectors = [VectorizeDocuments(doc, model) for doc in documents]
#     similarities = [cosine_similarity([query_vector], [doc_vector])[0][0] for doc_vector in doc_vectors]
#     ranked_docs = sorted(zip(doc_names, similarities), key=lambda x: x[1], reverse=True)
#     result = [{fileName:str(float(rank))} for fileName, rank in ranked_docs]
#     return result
def RankDocumentsMultipleQueries(query_words, documents, model, doc_names):
    result = {}
    for word in query_words:
        query_vector = model.wv[word] if word in model.wv else None
        if query_vector is None:
            result[word] = []
            continue

        doc_vectors = [VectorizeDocuments(doc, model) for doc in documents]
        similarities = [cosine_similarity([query_vector], [doc_vector])[0][0] for doc_vector in doc_vectors]
        ranked_docs = sorted(zip(doc_names, similarities), key=lambda x: x[1], reverse=True)
        result[word] = [{fileName: str(float(rank))} for fileName, rank in ranked_docs]
    return result


# 7. 文檔分群，顯示文件名稱，使用群組內高頻詞進行標籤化
def GenerateClusterLabels(clustered_docs, original_documents):
    cluster_labels = {}
    for cluster, doc_names in clustered_docs.items():
        # 對每個群組中的文件名稱對應的文本內容進行詞頻統計
        all_words = []
        for doc_name in doc_names:
            text_content = original_documents[doc_name]  # 從原始文件名稱映射的文本中提取分詞結果
            all_words.extend(text_content)
        word_counts = Counter(all_words)
        # 選取出現最多的3個關鍵詞作為群組標籤
        common_words = [word for word, _ in word_counts.most_common(5)]
        cluster_labels[cluster] = ' '.join(common_words)
    return cluster_labels

# 文檔分群並生成語意化標籤
def ClusterDocumentsWithLabels(documents, model, doc_names, num_clusters=5):
    doc_vectors = [VectorizeDocuments(doc, model) for doc in documents]
    kmeans = KMeans(n_clusters=num_clusters, random_state=0).fit(doc_vectors)
    
    clustered_docs = defaultdict(list)
    for idx, label in enumerate(kmeans.labels_):
        clustered_docs[label].append(doc_names[idx])  # 使用文件名稱而非索引
    
    # 生成群組語意化標籤
    # 建立 `original_documents` 作為 doc_name 到對應文件分詞結果的映射
    original_documents = {name: doc for name, doc in zip(doc_names, documents)}
    cluster_labels = GenerateClusterLabels(clustered_docs, original_documents)
    
    # 替換分群結果的 key 為語意化標籤
    labeled_clusters = {}
    for cluster, docs in clustered_docs.items():
        label = cluster_labels[cluster]
        labeled_clusters[label] = docs

    return labeled_clusters
# def ClusterDocumentsWithLabelsMultiQueries(documents, model, doc_names, query_words, num_clusters=5):
#     result = {}
#     for word in query_words:
#         doc_vectors = [VectorizeDocuments(doc, model) for doc in documents]
#         kmeans = KMeans(n_clusters=num_clusters, random_state=0).fit(doc_vectors)

#         clustered_docs = defaultdict(list)
#         for idx, label in enumerate(kmeans.labels_):
#             clustered_docs[label].append(doc_names[idx])

#         original_documents = {name: doc for name, doc in zip(doc_names, documents)}
#         cluster_labels = GenerateClusterLabels(clustered_docs, original_documents)

#         labeled_clusters = {}
#         for cluster, docs in clustered_docs.items():
#             label = cluster_labels[cluster]
#             labeled_clusters[label] = docs

#         result[word] = labeled_clusters
#     return result

def UseWord2Vec(jsonData, queryWords, matchMode, agmMode):
    model_path = r'C:\Users\Emma\Desktop\word2vec_model.model'  # 模型儲存路徑

    preprocessed_json = PreprocessDocuments(jsonData, queryWords, matchMode)
    preprocessed_data = json.loads(preprocessed_json)
    documents = []
    doc_names = []
    for item in preprocessed_data:
        doc_names.append(item['filePath'])
        documents.extend(item['documents'])
    # doc_names = [item['filePath'] for item in preprocessed_data]
    # 檢查模型是否存在，如無則訓練並儲存新模型
    # if os.path.exists(model_path):
    #     model = LoadWord2vecModel(model_path)
    # else:
    model = TrainWord2vecModel(documents, sg=agmMode, model_path=model_path)

    # 查詢擴展、排序與分群
    # queryWords = ["children", "virus", "infection"]
    expanded_queries = ExpandQuery(queryWords, model)
    ranked_docs_multiple = RankDocumentsMultipleQueries(queryWords, documents, model, doc_names)
    clustered_docs_with_labels_multiple = ClusterDocumentsWithLabels(documents, model, doc_names)

    word2vecData = {
        "ExpandedQueries":expanded_queries,
        "TopRanked":ranked_docs_multiple,
        "clusters":clustered_docs_with_labels_multiple
    }
    WriteFile("word2vec.json", word2vecData)
    # PlotWord2Vec()
    return [dic for fileContentData in preprocessed_data if (dic := TextContentByKeywords(fileContentData, queryWords, matchMode))]
#endregion

#region IR
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
 
def GetDefaultRetrievalResult(data):
    words = word_tokenize(data)
    words = [word for word in words if any((ord(char) > 47 and ord(char) <58) or (ord(char) > 65 and ord(char) < 91) or (ord(char) > 96 and ord(char) < 122) for char in word)]
    stopWords = set(stopwords.words('english'))
    charCountIncludingSpaces = len(data.replace("\r\n", ""))
    charCountExcludingSpaces = len(data.replace(" ", ""))
    wordsCount = len(words)
    sentences = sent_tokenize(data)
    sentenceCount = len(sentences)
    nonAsciiChars = [char for char in data if ord(char) > 127]
    nonAsciiWords = [word for word in words if any(ord(char) > 127 for char in word)]
    words = word_tokenize(data.lower())
    normalized_words = [freqW for freqW in words if freqW.isalpha() and freqW not in stopWords]
    frequency_distribution = FreqDist(normalized_words)
    frequencyWords = dict(frequency_distribution.most_common(wordsCount))
    baseJsonData = {
        "CharCountIncludingSpaces": charCountIncludingSpaces,
        "CharCountExcludingSpaces": charCountExcludingSpaces,
        "WordsCount": wordsCount,
        "SentenceCount": sentenceCount,
        "FrequencyWords": frequencyWords,
        "NonAsciiChars": len(nonAsciiChars),
        "NonAsciiWords": len(nonAsciiWords),
        "Content": data,
        "Stopwords": list(stopWords)
    }
    return baseJsonData

def GetStemResult(data):
    stemmer = SnowballStemmer('english')

    stemResult = [stemmer.stem(word) for word in word_tokenize(data)]

    stopWords = list(set(stopwords.words('english')))

    word_count_dict = {}

    for word in stemResult:
        if word not in string.punctuation and word not in stopWords:
            if word in word_count_dict:
                word_count_dict[word] += 1
            else:
                word_count_dict[word] = 1
            
    return dict(sorted(word_count_dict.items(), key=lambda x: (-x[1], x[0])))

def GetEditDistanceKeywords(keyword, content, threshold=2):
    words = word_tokenize(content.lower())

    keyword = keyword.lower()

    wordsSet = set()

    for word in words:
        edit_distance = Levenshtein.distance(keyword, word)
        
        if edit_distance <= threshold:
            wordsSet.add(word)

    return wordsSet

def GetContent(filePath):
    isXml = filePath.endswith('.xml')

    fileContent = ReadFile(filePath)

    content = ""

    if isXml:
        soup = BeautifulSoup(fileContent, 'lxml-xml')
        article = soup.find('Article')
        if article:
            abstracts = article.find_all('Abstract')
            content = '\r\n'.join(
                abstractText.get_text() 
                for abstract in abstracts 
                for abstractText in abstract.find_all('AbstractText', recursive=False)
            )
    else:
        content = fileContent
    
    return content

def TextCount(jsonData):
    if jsonData['content'] == "":
        return None

    stemResult = GetStemResult(jsonData['content'])
    
    defaultJsonData = GetDefaultRetrievalResult(jsonData['content'])

    resultDict = {
        "FileName": jsonData['filePath'],
        "FrequencyWordsWithStemming": stemResult
    }

    resultDict.update(defaultJsonData)

    return resultDict

# def TextContentByKeyword(irResult, keyword):
#     filePath = irResult['filePath']
#     content = irResult['content']

#     keywordSet = set()

#     if keyword and keyword.strip():
#         keywordSet = GetEditDistanceKeywords(keyword, content)
#         if len(keywordSet) == 0:
#             return None
        
#     stemResult = GetStemResult(content)
#     defaultJsonData = GetDefaultRetrievalResult(content)

#     resultDict = {
#         "FileName": filePath,
#         "FrequencyWordsWithStemming": stemResult
#     }

#     resultDict.update(defaultJsonData)

#     if len(keywordSet) > 0: 
#         resultDict.update({
#             "KeywordSetCount": len(keywordSet),
#             "keywordSet": list(keywordSet)
#         })
        
#     resultDict.update(defaultJsonData)

#     return resultDict
def TextContentByKeywords(irResult, keywords, match_mode):
    filePath = irResult['filePath']
    content = irResult['content']

    keywordSet = set()
    found_all_keywords = True  # 用於 AND 模式的檢查

    # 根據匹配模式進行關鍵字篩選
    if keywords:
        for keyword in keywords:
            matched_words = GetEditDistanceKeywords(keyword, content)
            
            # 在 OR 模式下，找到任一關鍵字即結束循環
            if match_mode == "OR" and matched_words:
                keywordSet.update(matched_words)
                break
            
            # 在 AND 模式下，若其中一個關鍵字無法匹配，則設置 found_all_keywords 為 False
            elif match_mode == "AND":
                if matched_words:
                    keywordSet.update(matched_words)
                else:
                    found_all_keywords = False
                    break
        
        # 若 AND 模式下有關鍵字未匹配，則直接返回 None
        if match_mode == "AND" and not found_all_keywords:
            return None
        
        # 如果 OR 模式下沒有匹配任何關鍵字，也返回 None
        if match_mode == "OR" and len(keywordSet) == 0:
            return None

    # 使用其他方法取得處理結果
    stemResult = GetStemResult(content)
    defaultJsonData = GetDefaultRetrievalResult(content)

    # 組裝結果
    resultDict = {
        "FileName": filePath,
        "FrequencyWordsWithStemming": stemResult
    }

    # 加入取得的預設檢索結果
    resultDict.update(defaultJsonData)

    # 若有匹配關鍵字，將結果更新到字典中
    if len(keywordSet) > 0: 
        resultDict.update({
            "KeywordSetCount": len(keywordSet),
            "keywordSet": list(keywordSet)
        })

    return resultDict

def IRResultFile(keywords, matchMode):
    jsonPath = os.path.join(dir, "defaultFiles.json")
    if not os.path.exists(jsonPath):
        print(f"檔案不存在: {jsonPath}")
    jsonData = ReadJson(jsonPath)
    if not jsonData:
        print(f"讀取初始分析檔案失敗")

    # fileContentList = jsonData['IRResult']

    return [dic for fileContentData in jsonData if (dic := TextContentByKeywords(fileContentData, keywords, matchMode))]

def Word2VecFile(keywords, matchMode, agmMode):
    jsonPath = os.path.join(dir, "defaultFiles.json")
    if not os.path.exists(jsonPath):
        print(f"檔案不存在: {jsonPath}")
    jsonData = ReadJson(jsonPath)
    if not jsonData:
        print(f"讀取初始分析檔案失敗")

    return UseWord2Vec(jsonData, keywords, matchMode, agmMode)

def IRAllFiles(filePaths):
    fileDataList = []
    irResults = []

    for filePath in filePaths:
        file_exists = os.path.exists(filePath)
        content = ""
        analyzed = False

        fileData = {
            "filePath": os.path.basename(filePath),
            "content": content,
            "fileStatus": file_exists,
            "analyzed": analyzed
        }

        if file_exists:
            fileData['content'] = GetContent(filePath)  # 使用 extract_content 方法
            fileInfo = TextCount(fileData)

            if fileInfo:
                fileData['analyzed'] = True
                irResults.append(fileInfo)

        fileDataList.append(fileData)

    # 將結果寫入JSON文件
    WriteFile("defaultFiles.json", fileDataList)
    
    return irResults

def CountWordFrequencies(data, tag):
    word_count = defaultdict(int)
    
    for item in data:
        for word, count in item[tag].items():
            word_count[word] += count
            
    return dict(sorted(word_count.items(), key=lambda x: (-x[1], x[0])))

def PlotWord2Vec(sg):
    agm = 'SG'
    if sg==0:
        agm = 'CBOW'
    jsonPath = os.path.join(dir, "word2vec.json")
    with open(jsonPath, "r") as f:
        data = json.load(f)

    # 假設你有一個已經訓練好的 Word2Vec 模型
    model_path = r"C:\Users\Emma\Desktop\word2vec_model.model"  # 修改為你的模型路徑
    model = Word2Vec.load(model_path)  # 加載模型
    word_vectors = model.wv  # 提取詞向量

    # 提取 "ExpandedQueries" 中的字詞
    words = []
    vectors = []

    for query, word_list in data["ExpandedQueries"].items():
        for word_dict in word_list:
            if isinstance(word_dict, dict) and len(word_dict) > 0:
                for word, similarity in word_dict.items():
                    # 確保該詞在模型中存在
                    if word in word_vectors:
                        words.append(word)
                        vectors.append(word_vectors[word])  # 使用詞向量

    # 將 vectors 標準化處理
    vectors = normalize(vectors)

    # 使用 KMeans 進行分群
    num_clusters = 5  # 設定你想要的群集數量
    kmeans = KMeans(n_clusters=num_clusters, random_state=42)
    kmeans.fit(vectors)
    labels = kmeans.labels_  # 取得每個詞的群集標籤


    # 使用 t-SNE 降維到 2D
    tsne = TSNE(n_components=2, random_state=42)
    reduced_vectors = tsne.fit_transform(vectors)

    # 可視化
    plt.figure(figsize=(10, 8))
    plt.scatter(reduced_vectors[:, 0], reduced_vectors[:, 1])

    # # 標註每個點的詞
    # for i, word in enumerate(words):
    #     plt.annotate(word, (reduced_vectors[i, 0], reduced_vectors[i, 1]))

    # plt.title("Word2Vec Similarity Visualization (t-SNE)")
    # plt.show()
        # 根據群集標籤為不同群集著色
    for i in range(num_clusters):
        # 篩選出屬於該群集的詞
        cluster_indices = [index for index, label in enumerate(labels) if label == i]
        cluster_words = [words[index] for index in cluster_indices]
        cluster_vectors = reduced_vectors[cluster_indices]

        # 為每個群集選擇不同的顏色
        plt.scatter(cluster_vectors[:, 0], cluster_vectors[:, 1], label=f"Cluster {i}")

        # 標註每個點的詞
        for j, word in enumerate(cluster_words):
            plt.annotate(word, (cluster_vectors[j, 0], cluster_vectors[j, 1]), fontsize=8)

    plt.title("Word2Vec Similarity Visualization with KMeans Clustering (t-SNE)")
    plt.legend()
    
    if dir:
        imagePath = os.path.join(dir, f"Word2Vec_{agm}.png")
        if os.path.exists(imagePath):
            os.remove(imagePath)
        plt.savefig(imagePath)
    # plt.show(block=False)
    # plt.close()
    return imagePath

def PlotZipfDistribution(frequency_words, title, fig_num=int):
    top_words = dict(list(frequency_words.items())[:80])

    # words, frequencies = zip(*top_words)
    # words = list(top_words.keys())
    words = [f"{word}[{freq}]" for word, freq in top_words.items()]  
    frequencies = list(top_words.values())
    
    plt.figure(fig_num, figsize=(16, 9)) 
    plt.plot(words, frequencies, color='blue', marker='o', linestyle='-', linewidth=2)

    for i, v in enumerate(frequencies):
        if i % 2 == 0:
            plt.text(i, v + 3, str(v), ha='center', fontweight='bold', va='bottom')
        else:
            plt.text(i, v - 3, str(v), ha='center', fontweight='bold', va='top', color='red')
        # plt.text(i, v + 1, '', ha='center', va='bottom')
        # plt.text(i, v + 1, str(v), ha='center', va='bottom')
        
    plt.title(title)
    plt.xlabel("Words")
    plt.ylabel("Frequency")
    plt.xticks(rotation=45, ha='right')
    plt.tight_layout()

    if dir:
        imagePath = os.path.join(dir, f"zipf_distribution_{title}.png")
        if os.path.exists(imagePath):
            os.remove(imagePath)
        plt.savefig(imagePath)
    # plt.show(block=False)
    plt.close()
    return imagePath

def GetInput():
    selectFiles = r'D:\TestDir\selectFiles.txt'  

    if not os.path.exists(selectFiles):
        print(f"檔案 {selectFiles} 不存在。")
        return f"檔案 {selectFiles} 不存在。"

    content = ReadFile(selectFiles)

    contentSplit = content.split('|')
    keywordData = contentSplit[0].split('&')
    filePaths = contentSplit[1].split('&')

    if keywordData and len(keywordData[0])!=0 and all(keyword.strip() != "" for keyword in keywordData[0].split(' ')):
        keywords = keywordData[0].split(' ')
        if keywordData[2] == 'true':
            result = Word2VecFile(keywords, keywordData[1], keywordData[3])
        else:
            result = IRResultFile(keywords, keywordData[1])
    else:
        jsonPath = os.path.join(dir, "result.json")
        if os.path.exists(jsonPath):
            defaultJsonData = ReadJson(jsonPath)
            result = defaultJsonData['IRResult']
        else:
            result = IRAllFiles(filePaths)
    
    frequencyResult = CountWordFrequencies(result, 'FrequencyWordsWithStemming')
    originFrequency = CountWordFrequencies(result, 'FrequencyWords')

    jsonData = {
        "IRResult" :result,
        "Frequencies" : frequencyResult
    }

    frequencyData = {
        "OriginFrequency":originFrequency,
        "CompareFrequency":frequencyResult
    }

    filePath = WriteFile("frequency.json", frequencyData)

    if not keywordData[0] or keywordData[0].strip() == "":
        filePath = WriteFile("result.json", jsonData)
        imagePath1 = PlotZipfDistribution(frequencyData["OriginFrequency"], "OriginFrequency",  fig_num=1)
        imagePath2 = PlotZipfDistribution(frequencyData["CompareFrequency"],  "CompareFrequency", fig_num=2)
        returnPath = f"{filePath}|{imagePath1}|{imagePath2}"
    else:
        filePath = WriteFile("keywordResult.json", jsonData)
        imagePath1 = PlotZipfDistribution(frequencyData["OriginFrequency"], "OriginFrequencyWithKeyword",  fig_num=1)
        imagePath2 = PlotZipfDistribution(frequencyData["CompareFrequency"],  "CompareFrequencyWithKeyword", fig_num=2)
        imagePath3 = PlotWord2Vec(keywordData[3])
        returnPath = f"{filePath}|{imagePath1}|{imagePath2}|{imagePath3}"

 

    print(returnPath, end='')
#endregion

GetInput()