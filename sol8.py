# LeNet-5
import matplotlib as mp ;
mp.use("Qt4Agg") ;
import gzip, pickle,numpy as np ;
import numpy.random as npr, tensorflow as tf;

mnistPath = "./mnist.pkl.gz"



sess = tf.Session();

with gzip.open(mnistPath, 'rb') as f:
  # python3
  #((traind,trainl),(vald,vall),(testd,testl))=pickle.load(f, encoding='bytes')
  # python2
  ((traind,trainl),(vald,vall),(testd,testl))=pickle.load(f, encoding="bytes") ;
  traind = traind.astype("float32").reshape(-1,784) ;
  trainl = trainl.astype("float32") ;
  testd = testd.astype("float32").reshape(-1,784) ;
  testl = testl.astype("float32") ;
data_placeholder = tf.placeholder(tf.float32,[None,784]) ;
label_placeholder = tf.placeholder(tf.float32,[None,10]) ;

# just sample N random elements
N = 200 ;
rawIndices = np.array(range(0,traind.shape[0])) ;
npr.shuffle(rawIndices);
indices = rawIndices[0:N] ;
fd = {data_placeholder: traind[indices], label_placeholder : trainl[indices] } ;

## reshape data tensor into NHWC format
dataReshaped=tf.reshape(data_placeholder, (-1,28,28,1)) ;
print (dataReshaped) ;

## Hidden Layer 1
# Convolution Layer with 32 fiters and a kernel size of 5
conv1 = tf.nn.relu(tf.layers.conv2d(dataReshaped,6, 5,name="H1")) ;
print (conv1) ;
# Max Pooling (down-sampling) with strides of 2 and kernel size of 2
a1 = tf.layers.max_pooling2d(conv1, 2, 2) ;
print (a1) ;

## Hidden Layer 2
# Convolution Layer with 64 filters and a kernel size of 3
conv2 = tf.nn.relu(tf.layers.conv2d(a1, 16, 5,name="H2")) ;
# Max Pooling (down-sampling) with strides of 2 and kernel size of 2
a2 = tf.layers.max_pooling2d(conv2, 2, 2) ;
print (a2) ;
a2flat = tf.reshape(a2, (-1,4*4*16)) ;

## Hidden Layer 3
Z3 = 120 ;
# allocate variables
W3 = tf.Variable(npr.uniform(-0.01,0.01, [4*4*16,Z3]),dtype=tf.float32, name ="W3") ;
b3 = tf.Variable(npr.uniform(-0.01,0.01, [1,Z3]),dtype=tf.float32, name ="b3") ;
# compute activations
a3 = tf.nn.relu(tf.matmul(a2flat, W3) + b3) ;
print (a3) ;

## Hidden Layer 4
Z4 = 84 ;
# allocate variables
W4 = tf.Variable(npr.uniform(-0.01,0.01, [Z3,Z4]),dtype=tf.float32, name ="W4") ;
b4 = tf.Variable(npr.uniform(-0.01,0.01, [1,Z4]),dtype=tf.float32, name ="b4") ;
# compute activations
a4 = tf.nn.relu(tf.matmul(a3, W4) + b4) ;
print (a4) ;


## output layer
# alloc variables
Z5 = 10 ;
W5 = tf.Variable(npr.uniform(-0.1,0.1, [Z4,Z5]),dtype=tf.float32, name ="W5") ;
b5 = tf.Variable(npr.uniform(-0.01,0.01, [1,Z5]),dtype=tf.float32, name ="b5") ;
# compute activations
logits = tf.matmul(a4, W5) + b5 ;
print (logits) ;

## loss
lossBySample = tf.nn.softmax_cross_entropy_with_logits_v2(logits=logits, labels=label_placeholder) ;
loss = tf.reduce_mean(lossBySample) ;

## classification accuracy
nrCorrect = tf.reduce_mean(tf.cast(tf.equal (tf.argmax(logits,axis=1), tf.argmax(label_placeholder,axis=1)), tf.float32)) ;

## create update op
optimizer = tf.train.GradientDescentOptimizer(learning_rate = 0.5) ;  # 0.00001
update = optimizer.minimize(loss) ;

## init all variables
sess.run(tf.global_variables_initializer()) ;

## learn!!
iteration = 0 ;
tMax = 1000;
for iteration in range(0,tMax):
  # update parameters
  sess.run(update, feed_dict = fd) ;
  correct, lossVal= sess.run([nrCorrect, loss], feed_dict = fd) ;
  testacc = sess.run(nrCorrect, feed_dict = {data_placeholder: testd, label_placeholder: testl})
  print ("epoch ", iteration, "acc=", float(correct), "loss=", lossVal, "testacc=",testacc) ;
















