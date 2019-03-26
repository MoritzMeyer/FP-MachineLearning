# LeNet-5
import gzip, pickle;
import numpy as np;
import tensorflow as tf;
import os ;
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

bigSamplesPath = "./bigSamples.pkl.gz";
testDataPath = "./testSamples.pkl.gz";

with gzip.open(bigSamplesPath, 'rb') as f:
  #python3
  #tmp = pickle.load(f, encoding='bytes')
  # python2
  tmp = pickle.load(f) ;

  trainlLane = tmp["trainlLane"]
  traind = tmp["trainlPictureBB_full"]

  # check input shapes
  print(trainlLane.shape)
  print(traind.shape)

with gzip.open(testDataPath, 'rb') as f:
  #python3
  #tmp = pickle.load(f, encoding='bytes')
  # python2
  tmp = pickle.load(f) ;

  testlLane = tmp["trainlLane"]
  testd = tmp["trainlPictureBB_full"]

  # check input shapes
  print(testlLane.shape)
  print(testd.shape)


# analyse data
# check for block sorting
print("are the bigSamples block sorted?", trainlLane.argmax(axis=1))
print("are the testSamples block sorted?", testlLane.argmax(axis=1));

# re-shuffle bigSamples
nrTrainSamples = traind.shape[0]
indices = np.arange(0,nrTrainSamples) # alle indices der trainsamples holen
np.random.shuffle(indices) # alles indices shufflen, muss nicht zugewiesen werden, da implace "ersetzend"
traind = traind[indices];
trainlLane = trainlLane[indices]
print ("shuffledIndices bigSamples", trainlLane.argmax(axis=1))

# re-shuffle testSamples
nrTrainSamples = testd.shape[0]
indices = np.arange(0, nrTrainSamples) # alle indices der trainsamples holen
np.random.shuffle(indices) # alles indices shufflen, muss nicht zugewiesen werden, da implace "ersetzend"
testd = testd[indices];
testlLane = testlLane[indices];
print ("shuffledIndices testSamples", trainlLane.argmax(axis=1))

# check min and max of pictures
dmin = traind.min(axis = 0) # hole die kleinste Zeile! wegen traind axis 0
dmax = traind.max(axis = 0)
print("min von min: ", dmin.min()) # hole das min aus der kleinsten Zeile
print("max von min: ", dmin.max()) # hole das min aus der kleinsten Zeile
print("max von max: ", dmax.min()) # max zischen 1 und 10000 --> hist plotten
print("max von max: ", dmax.max()) # max zischen 1 und 10000 --> hist plotten

# get index of min and max picture
print(dmax.argmax()) # finde index an welcher position 10000
print(dmin.argmax()) #selbe!

# check number of samples per class ong bigSamplesLabels
samplesPerClass = trainlLane.sum(axis=0);
print("samplesPerClass bigSamples", samplesPerClass);

# check number of samples per class on testlabels
samplesPerClass = testlLane.sum(axis=0);
print("samplesPerClass testSamples", samplesPerClass);


with tf.Session() as sess:
  ## input layer: Nx108x384x1
  dataPlaceholder = tf.placeholder(tf.float32, shape=[None, 108, 384, 1]);
  labelPlaceholder = tf.placeholder(tf.float32,[None, 8]) ;

  ## Hidden Layer 1: 104x380x6 => 52x190x6
  # Convolution Layer with 32 fiters and a kernel size of 5
  conv1 = tf.nn.relu(tf.layers.conv2d(dataPlaceholder,6, 5,name="H1")) ;
  print (conv1) ;
  a1 = tf.layers.max_pooling2d(conv1, 2, 2) ;
  print (a1) ;

  ## Hidden Layer 2: 48x186x16 => 24x93x16
  conv2 = tf.nn.relu(tf.layers.conv2d(a1, 16, 5,name="H2")) ;
  a2 = tf.layers.max_pooling2d(conv2, 2, 2) ;
  print (a2) ;
  a2flat = tf.reshape(a2, (-1, 24 * 93 * 16)) ;

  ## Hidden Layer 3
  Z3 = 120 ;
  # allocate variables
  W3 = tf.Variable(np.random.uniform(-0.01,0.01, [24 * 93 * 16,Z3]),dtype=tf.float32, name ="W3") ; # adjust weights for a2flat!
  b3 = tf.Variable(np.random.uniform(-0.01,0.01, [1,Z3]),dtype=tf.float32, name ="b3") ;
  # compute activations
  a3 = tf.nn.relu(tf.matmul(a2flat, W3) + b3) ;
  print (a3) ;

  ## Hidden Layer 4
  Z4 = 84 ;
  # allocate variables
  W4 = tf.Variable(np.random.uniform(-0.01,0.01, [Z3,Z4]),dtype=tf.float32, name ="W4") ;
  b4 = tf.Variable(np.random.uniform(-0.01,0.01, [1,Z4]),dtype=tf.float32, name ="b4") ;
  # compute activations
  a4 = tf.nn.relu(tf.matmul(a3, W4) + b4) ;
  print (a4) ;


  ## output layer
  # alloc variables
  Z5 = 8 ;
  W5 = tf.Variable(np.random.uniform(-0.1,0.1, [Z4,Z5]),dtype=tf.float32, name ="W5") ;
  b5 = tf.Variable(np.random.uniform(-0.01,0.01, [1,Z5]),dtype=tf.float32, name ="b5") ;
  # compute activations
  logits = tf.matmul(a4, W5) + b5 ;
  print (logits) ;

  ## loss
  lossBySample = tf.nn.softmax_cross_entropy_with_logits_v2(logits=logits, labels=labelPlaceholder) ;
  loss = tf.reduce_mean(lossBySample) ;

  ## classification accuracy
  nrCorrect = tf.reduce_mean(tf.cast(tf.equal (tf.argmax(logits,axis=1), tf.argmax(labelPlaceholder,axis=1)), tf.float32)) ;

  ## create update op
  optimizer = tf.train.GradientDescentOptimizer(learning_rate = 0.005) ;  # 0.00001
  update = optimizer.minimize(loss) ;

  ## init all variables
  sess.run(tf.global_variables_initializer()) ;

  ## train!!

  # variables and constants related to the drawing of batches
  nrTrainSamples = traind.shape[0] ;
  batchIndex = -1 ;
  batchSize = 100 ;
  maxBatchIndex = nrTrainSamples // batchSize ;
  nrEpochs = 0 ;

  iteration = 0 ;
  tMax = 2000;
  for iteration in range(0,tMax):

    # if we have exceeded the size of traind, restart!
    if batchIndex >= maxBatchIndex:
      batchIndex=-1 ;
      nrEpochs += 1;

    # draw batches
    batchIndex +=1 ;
    dataBatch = traind[batchIndex * batchSize:(batchIndex+1) * batchSize] ;
    labelBatch = trainlLane[batchIndex * batchSize:(batchIndex+1) * batchSize] ;

    # update CNN parameters
    sess.run(update, feed_dict = {dataPlaceholder: dataBatch, labelPlaceholder : labelBatch}) ;

    # compute loss and accuracy, only every 50 iterations to save time
    if iteration%50==0:
      acc, lossVal= sess.run([nrCorrect, loss], feed_dict =  {dataPlaceholder: dataBatch, labelPlaceholder : labelBatch}) ;
      testacc = sess.run(nrCorrect, feed_dict = {dataPlaceholder: testd, labelPlaceholder: testlLane})
      print ("epoch=", nrEpochs, "iteration=", iteration, ", acc=", float(acc), "loss=", lossVal, "testacc=",testacc) ;
