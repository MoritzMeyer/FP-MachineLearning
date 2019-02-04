# LeNet-5
import matplotlib as mp ;
mp.use("Qt4Agg") ;
import gzip, pickle;
#import gzip, pickle,numpy as np, matplotlib.pyplot as plt ;
import numpy as np;
import tensorflow as tf;
import numpy.random as npr;
#import matplotlib.pyplot as plt;

mnistPath = "./mnist.pkl.gz"



sess = tf.Session();

with gzip.open("gulasch.pkl.gz", 'rb') as handle:
    data = pickle.load(handle)

    # box mit 3750 x 5
    trainlBox = data["trainlBox"]
    trainlLane = data["trainlLane"]
    traind = data["traind"]

    print(trainlBox.shape)
    print(trainlLane.shape)
    print(traind.shape)
#data_placeholder = tf.placeholder(tf.float32,[None,784]) ;
#label_placeholder = tf.placeholder(tf.float32,[None,10]) ;
dataPlaceholder = tf.placeholder(tf.float32, shape=[None, 100, 120, 1]);
labelPlaceholder = tf.placeholder(tf.float32, shape=[None, 8]);
boxPlaceholder = tf.placeholder(tf.float32, shape=[None, 8]);

# just sample N random elements
N = 1000;
rawIndices = np.array(range(0,traind.shape[0])) ;
npr.shuffle(rawIndices);
indices = rawIndices[0:N] ;
fd = {dataPlaceholder: traind[indices], labelPlaceholder : trainlLane[indices], boxPlaceholder: trainlBox[indices] } ;

## reshape data tensor into NHWC format
reshapedData = tf.reshape(dataPlaceholder, (-1, 100, 120, 1));
print (reshapedData) ;

## Hidden Layer 1
# Convolution Layer with 32 fiters and a kernel size of 5
conv1 = tf.nn.relu(tf.layers.conv2d(reshapedData,6, 5,name="H1")) ;
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
a2flat = tf.reshape(a2, (-1, 22 * 27 * 16)) ;

# put bounding boxes behind
a2flat = tf.concat([a2flat, boxPlaceholder], axis=1);
print("a2flat.shape: ", a2flat.shape);

## Hidden Layer 3
Z3 = 120 ;
# allocate variables
W3 = tf.Variable(npr.uniform(-0.01,0.01, [22 * 27 * 16 + 8,Z3]),dtype=tf.float32, name ="W3") ;
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
Z5 = 8 ;
W5 = tf.Variable(npr.uniform(-0.1,0.1, [Z4,Z5]),dtype=tf.float32, name ="W5") ;
b5 = tf.Variable(npr.uniform(-0.01,0.01, [1,Z5]),dtype=tf.float32, name ="b5") ;
# compute activations
logits = tf.matmul(a4, W5) + b5 ;
print (logits) ;
print ("logits shape", logits.shape);
## loss
lossBySample = tf.nn.softmax_cross_entropy_with_logits_v2(logits=logits, labels=labelPlaceholder) ;
loss = tf.reduce_mean(lossBySample) ;

## classification accuracy
nrCorrect = tf.reduce_mean(tf.cast(tf.equal (tf.argmax(logits,axis=1), tf.argmax(labelPlaceholder,axis=1)), tf.float32)) ;

## create update op
optimizer = tf.train.GradientDescentOptimizer(learning_rate = 0.2) ;  # 0.00001
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
  #testacc = sess.run(nrCorrect, feed_dict = {data_placeholder: testd, label_placeholder: testl})
  print ("epoch ", iteration, "acc=", float(correct), "loss=", lossVal) ;
















