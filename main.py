import gzip, pickle;

with gzip.open('mnist.pkl.gz', 'rb') as f:
    ((traind, train1), (vald, vall), (testd, testl)) = pickle.load(f, encoding='latin1');
    traind = traind.astype("float32").reshape(-1, 784);
    train1 = train1.astype("float32");
    testd = testd.astype("float32").reshape(-1, 784);
    testl = testl.astype("float32");
import tensorflow as tf;
import numpy.random as npr;
import matplotlib.pyplot as plt;

# 1. 32x32x1 input
# 2. 28x28x6 after convLayer
# 3. 14x14x6 after maxPooling
# 4. 10x10x16 after convLayer
# 5. 5x5x16 after maxPooling
# 6. 120 after fcLayer
# 7. 84 after fcLayer
# 8. 10 after linear softmax MC layer
# Filter sizes sind immer bei den conv Layers: 5x5
# Durch die Formeln fuer H(L) und W(L) = W(l-1) - delta fx(l) / step(delta x(l)
# Bei h dann alles mit y.
# Pooling immer mit 2(2)

# c) Input sizes starten mit 28x28 und muessen dann umgerechnet werden
# 24x24x1 => 12x12x6 => 8x8x6=>4x4x16=>120=>84=>10

with tf.Session() as sess:
    dataPlaceholder = tf.placeholder(tf.float32, shape=[None, 784]);
    labelPlaceholder = tf.placeholder(tf.float32, shape=[None, 10]);

    N = 10000;
    fd = {dataPlaceholder: traind[0:N], labelPlaceholder: train1[0:N]};

    # 1a (Data wird Input von 1 mit 28x28 => NHWC Format
    reshapedData = tf.reshape(dataPlaceholder, (-1, 28, 28, 1));
    print(reshapedData)

    # Conv Layer 1 (6 Channel, Filters 5x5)
    conv1 = tf.nn.relu(tf.layers.conv2d(reshapedData, 6, 5, name="H1"));
    a1 = tf.layers.max_pooling2d(conv1, 2, 2);
    print(a1);

    # Conv Layer 2 (16 Channels, Filters 5x5)
    conv2 = tf.nn.relu(tf.layers.conv2d(a1, 16, 5, name="H2"));
    a2 = tf.layers.max_pooling2d(conv2, 2, 2);
    a2flat = tf.reshape(a2, (-1, 4 * 4 * 16));
    print(a2)

    # fcLayer1
    Z = 120
    # allocate variables
    W3 = tf.Variable(npr.uniform(-0.01, 0.01, [4 * 4 * 16, Z]), dtype=tf.float32, name="W3");
    b3 = tf.Variable(npr.uniform(-0.01, 0.01, [1, Z]), dtype=tf.float32, name="b3");
    # compute activations
    a3 = tf.nn.relu(tf.matmul(a2flat, W3) + b3);
    print(a3)
    # fcLayer2
    Z2 = 84
    # allocate variables
    W4 = tf.Variable(npr.uniform(-0.01, 0.01, [Z, Z2]), dtype=tf.float32, name="W4");
    b4 = tf.Variable(npr.uniform(-0.01, 0.01, [1, Z2]), dtype=tf.float32, name="b4");
    # compute activations
    a4 = tf.nn.relu(tf.matmul(a3, W4) + b4);
    print(a4)

    # output layer
    Z3 = 10

    # allocate variables
    W5 = tf.Variable(npr.uniform(-0.01, 0.01, [Z2, Z3]), dtype=tf.float32, name="W4");
    b5 = tf.Variable(npr.uniform(-0.01, 0.01, [1, Z3]), dtype=tf.float32, name="b4");

    logits = tf.matmul(a4, W5) + b5;

    ## loss
    lossBySample = tf.nn.softmax_cross_entropy_with_logits_v2(logits=logits, labels=labelPlaceholder);
    loss = tf.reduce_mean(lossBySample);

    ## classification accuracy
    nrCorrect = tf.reduce_mean(
        tf.cast(tf.equal(tf.argmax(logits, axis=1), tf.argmax(labelPlaceholder, axis=1)), tf.float32));

    ## create update op
    optimizer = tf.train.GradientDescentOptimizer(learning_rate=0.2);  # 0.00001
    update = optimizer.minimize(loss);

    ## init all variables
    sess.run(tf.global_variables_initializer());

    ## learn!!
    iteration = 0;
    tMax = 1000;
    for iteration in range(0, tMax):
        # update parameters
        sess.run(update, feed_dict=fd);
        correct, lossVal = sess.run([nrCorrect, loss], feed_dict=fd);
        testacc = sess.run(nrCorrect, feed_dict={dataPlaceholder: testd, labelPlaceholder: testl})
        print("epoch ", iteration, "acc=", float(correct), "loss=", lossVal, "testacc=", testacc);

    ## download layer 1 weights
    globVars = tf.get_collection(tf.GraphKeys.GLOBAL_VARIABLES);
    filtersH1 = [v for v in globVars if v.name == "H1/kernel:0"][0];
    filtersH1np = sess.run(filtersH1);
    print(filtersH1np.min(), filtersH1np.max());

    f, axesplt = plt.subplots(nrows=4, ncols=8);
    i = 0;
    for ax in axesplt.ravel():
        ax.imshow(filtersH1np[:, :, 0, i], cmap=plt.get_cmap("bone"));
        i += 1;
    plt.show()

    ## visualize result on test data
    testout = sess.run(logits, feed_dict={dataPlaceholder: testd});